using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NmsDotnet.Database
{
    internal interface IDao
    {
    }

    internal abstract class AbstractDAO<T> : IDao
    {
        public T Select(T entity)
        {
            return default(T);
        }

        public T SelectByOne(int id)
        {
            return default(T);
        }

        //Insert
        public int Insert(T entity)
        {
            return 0;
        }

        //Update
        public int Update(T entity)
        {
            return 0;
        }

        public int Delete(T entity)
        {
            return 0;
        }
    }
}