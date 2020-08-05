using System;
using System.Collections.Generic;
using System.Text;
using LevelDB;

namespace Storage.LevelDB
{
    public class LevelDBClient
    {
        private readonly DB m_levelDB = null;

        public static LevelDBClient Create(string dbName)
        {
            return new LevelDBClient(dbName);
        }

        private LevelDBClient(string dbName)
        {
            var options = new Options { CreateIfMissing = true };
            m_levelDB = new DB(options, dbName);
        }

        public string Get(string key)
        {
            return m_levelDB.Get(key);
        }

        public void Put(string key, string value)
        {
            m_levelDB.Put(key, value);
        }

        public void Delete(string key)
        {
            m_levelDB.Delete(key);
        }
    }
}
