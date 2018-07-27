using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RawSql.Models
{
    public class MemberContext
    {

        //public Database Db { get; set; }
        public string MemberTable = "member";
        private string connectionString { get; set; }
        public string Salt = "secret";

        public MemberContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
            //return Db.Connection;
        }

        public async Task<bool> VerifyPassword(string username, string password)
        {
            var query = $"SELECT `Password` FROM {MemberTable} WHERE `Username`=@Username";
            List<string> p = new List<string>();

            using(MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Username",
                    DbType = System.Data.DbType.String,
                    Value = username
                });

                //MySqlCommand cmd = new MySqlCommand("SELECT `Password` FROM member WHERE `Username`='user'", conn);
                string storedPassword = null;
                using (var reader = await cmd.ExecuteReaderAsync())
                { 
                    while (reader.Read())
                    {
                        p.Add(reader["Password"].ToString());
                    }

                    if (p.Count == 0) return false;

                    storedPassword = p[0];

                    if (storedPassword == passwordHash(password)) return true;
                }
            }
            return false;
        }

        public async Task<bool> VerifySession(string username, string clientSession)
        {
            var query = $"select `Session_id` from {MemberTable} where `Username` = @Username";

            using(MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Username",
                    DbType = System.Data.DbType.String,
                    Value = username
                });

                List<string> session = new List<string>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while(reader.Read())
                    {
                        session.Add(reader["Session_id"].ToString());
                    }

                    if (session.Count == 0) return false;

                    if (clientSession == session[0]) return true;
                    else return false;
                }
            }
        }

        public async Task<string> WriteSession(string username)
        {
            string session_id = createSession(username);

            var query = $"update `{MemberTable}` set `Session_id`= @session_id where `Username`= @Username ";
            //var query = "update `member` set `Session_id`='abc' where `Username`='user'";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                BindParam(cmd, session_id, username);

                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }

            return session_id;
        }

        public async Task CreateUser(Member member)
        {
            var query = $"insert into `{MemberTable}`(`Username`, `Password`) values ( @Username , @Password )";

            using(var conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", member.Username);
                cmd.Parameters.AddWithValue("@Password", passwordHash(member.Password));

                await cmd.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }
        }

        public async Task<List<Member>> GetMembers()
        {
            var query = $"select `MemberId`, `Username` from {MemberTable} where 1";
            List<Member> members = new List<Member>();

            using(var conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        members.Add(new Member
                        {
                            MemberId = Convert.ToInt32(reader["MemberId"]),
                            Username = reader["Username"].ToString(),
                            Password = string.Empty
                        });
                    }
                }

                await conn.CloseAsync();
            }

            return members;
        }

        private string createSession(string username)
        {
            using (var sha = new SHA256Managed())
            {
                string raw = username + Salt + DateTime.Now.ToShortTimeString();
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(raw);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        private string passwordHash(string password)
        {
            using (var sha = new SHA256Managed())
            {
                string raw = password;
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(raw);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        private void BindParam(MySqlCommand cmd, string session_id, string username)
        {
            cmd.Parameters.Add(new MySqlParameter {
                    ParameterName = "@session_id",
                    DbType = System.Data.DbType.String,
                    Value = session_id
                }        
            );
            cmd.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Username",
                    DbType = System.Data.DbType.String,
                    Value = username
                }
            );
        }
    }
}
