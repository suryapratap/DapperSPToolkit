// this is a special file that is meant to be used within the 
// Builder as well as during runtime, modify with care
// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable PossibleIntendedRethrow
namespace Dapper.Sp.Helper
{
    
    public interface IStoredProcedureInput
    {
    }
    
    public class DatabaseDbWrapper : IDatabaseWrapper
    {
        private System.Data.IDbTransaction _transaction;

        public DatabaseDbWrapper(string connStr)
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            Connection = new  System.Data.SqlClient. SqlConnection(connStr);
            Exception = null;
        }

        public System.Data.IDbConnection Connection { get; private set; }
        public bool WillRollBackTransaction => Exception != null;
        public bool HasTransaction => _transaction != null;
        public System.Exception Exception { get; private set; }


        public  System.Data.IDataReader ExecuteDr(string sql, IStoredProcedureInput input)
        {
            try
            {
                var d = input.ToDynamicPrarameters();
                var reader = Connection.ExecuteReader(sql, d, _transaction, commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: Connection.ConnectionTimeout);
                input.UpdateOutParams(d);
                return reader;
            }
            catch (System.Exception e)
            {
                Exception = e;
                throw e;
            }
        }

        public void Execute(string sql, IStoredProcedureInput input)
        {
            try
            {
                var d = input.ToDynamicPrarameters();
                Connection.Execute(sql, d, _transaction, commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: Connection.ConnectionTimeout);
                input.UpdateOutParams(d);
            }
            catch (System.Exception e)
            {
                Exception = e;
                throw e;
            }
        }


        public TOut ExecuteSingle<TOut>(string sql, IStoredProcedureInput input)
        {
            try
            {
                var d = input.ToDynamicPrarameters();
                var o = Connection.QueryFirstOrDefault<TOut>(sql, d, _transaction,
                    commandType: System.Data. CommandType.StoredProcedure, commandTimeout: Connection.ConnectionTimeout);
                input.UpdateOutParams(d);
                return o;
            }
            catch (System.Exception e)
            {
                Exception = e;
                throw e;
            }
        }


        public System.Collections.Generic.IEnumerable<TOut> ExecuteList<TOut>(string sql, IStoredProcedureInput input)
        {
            try
            {
                var d = input.ToDynamicPrarameters();
                var o = Connection.Query<TOut>(sql, d, _transaction, commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: Connection.ConnectionTimeout);
                input.UpdateOutParams(d);
                return o;
            }
            catch (System.Exception e)
            {
                Exception = e;
                throw e;
            }
        }


        public void MultiResult(string sql, IStoredProcedureInput input, System.Action<Dapper.SqlMapper.GridReader> mapResult)
        {
            try
            {
                var d = input.ToDynamicPrarameters();
                var o = Connection.QueryMultiple(sql, d, _transaction, commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: Connection.ConnectionTimeout);
                input.UpdateOutParams(d);
                mapResult(o);
            }
            catch (System.Exception e)
            {
                Exception = e;
                throw e;
            }
        }

        public T MultiResult<T>(string sql, IStoredProcedureInput input, System.Func<Dapper.SqlMapper.GridReader, T> mapResult)
        {
            try
            {
                var d = input.ToDynamicPrarameters();
                var o = Connection.QueryMultiple(sql, d, _transaction, commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: Connection.ConnectionTimeout);
                input.UpdateOutParams(d);
                return mapResult(o);
            }
            catch (System.Exception e)
            {
                Exception = e;
                throw e;
            }
        }

        public System.IDisposable BeginTransaction()
        {
            if (_transaction == null)
            {
                _transaction = Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
                return _transaction;
            }

            return null;
        }

        public void CleanUp(System.IDisposable tran)
        {
            if (tran != null)
            {
                if (_transaction != null)
                {
                    if (WillRollBackTransaction)
                        _transaction.Rollback();
                    else
                        _transaction.Commit();
                    _transaction = null;
                }

                tran.Dispose();
                tran = null;
            }

            if (Connection == null) return;

            if (Connection.State != System.Data.ConnectionState.Closed) Connection.Close();
            Connection.Dispose();
            Connection = null;
        }

        public Sp<T> GetSp<T>() where T : IStoredProcedureInput, new()
        {
            return GetSp(new T());
        }

        public Sp<T> GetSp<T>(T input) where T : IStoredProcedureInput
        {
            return Sp<T>.From(input, this);
        }

        public Sp<T> GetSp<T>(System.Func<T> hydrate) where T : IStoredProcedureInput
        {
            return GetSp(hydrate());
        }

        public Sp<T> GetSp<T>(System.Action<T> hydrate) where T : IStoredProcedureInput, new()
        {
            var sp = GetSp<T>();
            hydrate?.Invoke(sp.Input);
            return sp;
        }
    }
    
    public interface IDatabaseWrapper
    {
        bool WillRollBackTransaction { get; }
        bool HasTransaction { get; }
        System.Exception Exception { get; }
       System.Data.IDbConnection Connection { get; }
        System. IDisposable BeginTransaction();

        void Execute(string sql, IStoredProcedureInput input);
        System.Data.IDataReader ExecuteDr(string sql, IStoredProcedureInput input);
        TOut ExecuteSingle<TOut>(string sql, IStoredProcedureInput input);

        System.Collections.Generic.IEnumerable<TOut> ExecuteList<TOut>(string sql, IStoredProcedureInput input);
        void MultiResult(string sql, IStoredProcedureInput input, System.Action<SqlMapper.GridReader> mapResult);
        T MultiResult<T>(string sql, IStoredProcedureInput input,System. Func<SqlMapper.GridReader, T> mapResult);

        Sp<T> GetSp<T>() where T : IStoredProcedureInput, new();
        Sp<T> GetSp<T>(T input) where T : IStoredProcedureInput;
        Sp<T> GetSp<T>(System.Action<T> hydrate) where T : IStoredProcedureInput, new();
        Sp<T> GetSp<T>(System.Func<T> hydrate) where T : IStoredProcedureInput;
        void CleanUp(System.IDisposable transaction);
    }
    
    public interface IStoredProcedure<T> where T : IStoredProcedureInput
    {
        IDatabaseWrapper Wrapper { get; }
        T Input { get; }
        string GetSp();
        IStoredProcedure<T> SetWrapper(IDatabaseWrapper wrapper);
        IStoredProcedure<T> SetInput(T input);
    }
    
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    public class OutParameterAttribute : System.Attribute
    {
    }
    
    internal class PInfo
    {
        public PInfo(System.Reflection.PropertyInfo prop, bool isOut)
        {
            Prop = prop;
            IsOut = isOut;
        }

        public System.Reflection.PropertyInfo Prop { get;  }
        public bool IsOut { get;  }
        
    }
    
     public sealed class Sp<T> : IStoredProcedure<T> where T : IStoredProcedureInput
    {
        private static readonly System.Collections.Generic.Dictionary<System.Type, string> _procName = new System.Collections.Generic.Dictionary<System.Type, string>();

        private Sp()
        {
        }

        public IDatabaseWrapper Wrapper { get; private set; }

        public T Input { get; private set; }

        public string GetSp()
        {
            // ReSharper disable once HeapView.PossibleBoxingAllocation
            var t = Input.GetType();
            if (!_procName.ContainsKey(t))
            {
                var spName = t.Name;

                // check if we have a custom name specified by
                // application of StoredProcedureAttribute
                var att = (Dapper.Sp.Helper.StoredProcedureAttribute) System.Attribute.GetCustomAttribute(t, typeof(Dapper.Sp.Helper.StoredProcedureAttribute));
                if (att != null) spName = att.Name;

                _procName[t] = spName;
            }

            return _procName[t];
        }

        public IStoredProcedure<T> SetWrapper(IDatabaseWrapper wrapper)
        {
            Wrapper = wrapper;
            return this;
        }

        public IStoredProcedure<T> SetInput(T input)
        {
            Input = input;
            return this;
        }

        public static Sp<T> From(T input, IDatabaseWrapper wrapper)
        {
            var sp = new Sp<T>();
            sp.SetWrapper(wrapper).SetInput(input);
            return sp;
        }

        #region SP Input Heplers

        public void Execute()
        {
            Wrapper.Execute(GetSp(), Input);
        }

        public System.Data.IDataReader ExecuteDr()
        {
            return Wrapper.ExecuteDr(GetSp(), Input);
        }

        public TOut ExecuteSingle<TOut>()
        {
            return Wrapper.ExecuteSingle<TOut>(GetSp(), Input);
        }

        public System.Collections.Generic. IEnumerable<TOut> ExecuteList<TOut>()
        {
            return Wrapper.ExecuteList<TOut>(GetSp(), Input);
        }

        public void MultiResult(System.Action<SqlMapper.GridReader> mapResult)
        {
            Wrapper.MultiResult(GetSp(), Input, mapResult);
        }

        public TOut MultiResult<TOut>(System.Func<SqlMapper.GridReader, TOut> mapResult)
        {
            return Wrapper.MultiResult(GetSp(), Input, mapResult);
        }

        #endregion SP Input Heplers
    }
     
      public static class SqlWrapperEx
    {
        private static readonly System.Collections.Generic. Dictionary<System.Type, System.Collections.Generic.Dictionary<string, PInfo>> _propertyMap = new System.Collections.Generic. Dictionary<System.Type, System.Collections.Generic.Dictionary<string, PInfo>>();


        private static readonly System.Collections.Generic.Dictionary<string,System.Reflection.MethodInfo> _outParameterGetter = new System.Collections.Generic.Dictionary<string,System.Reflection.MethodInfo>();
        private static readonly System.Reflection. MethodInfo _getValue = typeof(Dapper.DynamicParameters).GetMethod("Get");

        private static System.Collections.Generic.Dictionary<string, PInfo> GetParameters<T>(this T instance) where T : IStoredProcedureInput
        {
            var type = instance.GetType();
            if (!_propertyMap.ContainsKey(type))
            {
                var props = type.GetProperties();
                var dict = new System.Collections.Generic.Dictionary<string, PInfo>();
                foreach (var prop in props)
                {
                    var isOut = prop.IsDefined(typeof(OutParameterAttribute),true);
                    dict[prop.Name] = new PInfo(prop, isOut);
                }

                _propertyMap[type] = dict;
            }

            return _propertyMap[type];
        }

        public static DynamicParameters ToDynamicPrarameters<T>(this T input) where T : IStoredProcedureInput
        {
            var pars = new DynamicParameters();
            var map = input.GetParameters();
            foreach (var item in map)
            {
                var name = item.Key;
                var value = item.Value.Prop.GetValue(input);
                var isString = item.Value.Prop.PropertyType.Name == "String";
                var size = isString ? (int?) -1 : null;
                if (item.Value.IsOut)
                    pars.Add(name, value, direction: System.Data.ParameterDirection.Output, size: size);
                else
                    pars.Add(name, value, size: size);
            }

            return pars;
        }

        public static void UpdateOutParams<T>(this T input, DynamicParameters wrapper) where T : IStoredProcedureInput
        {
            var map = input.GetParameters();
            foreach (var item in map)
                if (item.Value.IsOut)
                {
                    var name = item.Key;
                    if (!_outParameterGetter.ContainsKey(name))
                    {
                        var type = item.Value.Prop.PropertyType;
                        // ReSharper disable once HeapView.ObjectAllocation
                        var method = _getValue.MakeGenericMethod(type);
                        _outParameterGetter[name] = method;
                    }

                    object[] param = {name};
                    var value = _outParameterGetter[name].Invoke(wrapper, param);
                    item.Value.Prop.SetValue(input, value);
                }
        }

        public static System.Data.IDbConnection EnsureOpen(this System.Data.IDbConnection conn)
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
                return conn;
            }

            return null;
        }
    }
      
      [System.AttributeUsage(System.AttributeTargets.Class)]
      public class StoredProcedureAttribute : System.Attribute
      {
          public readonly string Name;

          public StoredProcedureAttribute(string name)
          {
              Name = name;
          }
      }
    
    
    
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {
    }
}