using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RawSql.Models
{
    public class Context
    {
        //public Database Db { get; set; }
        public string ConnectionString { get; set; }
        public string AlbumTable = "Album";


        public Context(String ConnectionString)
        {
            //this.Db = Db;
            this.ConnectionString = ConnectionString;
        }

        private MySqlConnection GetConnection()
        {
            //return Db.Connection;
            return new MySqlConnection(ConnectionString);
        }

        public async Task<List<Album>> GetAllAlbums()
        {
            List<Album> list = new List<Album>();

            using(MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand($"select * from {AlbumTable}", conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while(reader.Read())
                    {
                        list.Add(new Album()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            ArtistName = reader["ArtistName"].ToString(),
                            Price = Convert.ToInt32(reader["Price"]),
                            Genre = reader["genre"].ToString()
                        });
                    }
                }
                await conn.CloseAsync();
            }

            return list;
        }

        public async Task<Album> GetAlbum(int id)
        {
            Album result = new Album();
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand($"select * from {AlbumTable} where Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new Album()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            ArtistName = reader["ArtistName"].ToString(),
                            Price = Convert.ToInt32(reader["Price"]),
                            Genre = reader["genre"].ToString()
                        };
                    }
                }
            }

            return result;
        }

        public async Task CreateAlbum(Album album)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"insert into {AlbumTable}(`Name`, `ArtistName`, `Price`, `Genre`) values (@Name, @ArtistName, @Price, @Genre)", conn);

                BindParam(cmd, album);

                //var query = $"insert into {AlbumTable}(`Name`, `ArtistName`, `Price`, `Genre`) values ('{album.Name}', '{album.ArtistName}', {album.Price}, '{album.Genre}')";
                //MySqlCommand cmd = new MySqlCommand(query, conn);

                await cmd.ExecuteNonQueryAsync();

                //using (var reader = cmd.ExecuteReader())
                //{
                //    while (reader.Read())
                //    {
                        
                //    }
                //}
            }
            return;
        }

        void BindParam(MySqlCommand cmd, Album album)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Name",
                DbType = System.Data.DbType.String,
                Value = album.Name
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@ArtistName",
                DbType = System.Data.DbType.String,
                Value = album.ArtistName
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Price",
                DbType = System.Data.DbType.Int32,
                Value = album.Price
            });

            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@Genre",
                DbType = System.Data.DbType.String,
                Value = album.Genre
            });
        }
    }
}
