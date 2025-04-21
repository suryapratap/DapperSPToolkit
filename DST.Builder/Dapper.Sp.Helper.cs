// this is a special file that is meant to be used within the 
// Builder as well as during runtime, modify with care
// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable PossibleIntendedRethrow
namespace Dapper.Sp.Helper
{
    
    /// <summary>
 /// Represents the input parameters required for executing a stored procedure.
 /// </summary>
 /// <remarks>
 /// This interface serves as a contract for defining the structure of input parameters
 /// that are passed to stored procedures. Implementations of this interface should
 /// encapsulate the necessary data and provide any required validation or transformation
 /// logic.
 /// </remarks>
 public interface IStoredProcedureInput
 {
 }
 /// <summary>
 /// Provides a wrapper for database operations, including executing stored procedures,
 /// managing transactions, and handling connections.
 /// </summary>
 /// <remarks>
 /// This class facilitates interaction with a database by offering methods to execute
 /// stored procedures and retrieve results in various formats. It also supports
 /// transaction management and ensures proper cleanup of resources.
 /// </remarks>
 public class DatabaseDbWrapper(string connStr) : IDatabaseWrapper
 {
     private IDbTransaction? _transaction;

     // ReSharper disable once HeapView.ObjectAllocation.Evident

     public IDbConnection? Connection { get; private set; } = null;
     public bool WillRollBackTransaction => Exception != null;
     public bool HasTransaction => _transaction != null;
     public Exception? Exception { get; private set; } = null;


     private TOut ExecuteSafe<TOut>(IStoredProcedureInput input, Func<IDbConnection, SqlMapper.IDynamicParameters, TOut> command)
     {
         Connection ??= new SqlConnection(connStr);
         try
         {
             var d = input.ToDynamicPrarameters();
             var result = command(Connection, d);
             input.UpdateOutParams(d);
             return result;
         }
         catch (Exception e)
         {
             Exception = e;
             throw e;
         }
     }
     private void ExecuteSafe(IStoredProcedureInput input, Action<IDbConnection, SqlMapper.IDynamicParameters> command)
     {
         Connection ??= new SqlConnection(connStr);
         try
         {
             var d = input.ToDynamicPrarameters();
             command(Connection, d);
             input.UpdateOutParams(d);
         }
         catch (Exception e)
         {
             Exception = e;
             throw e;
         }
     }
     public IDataReader ExecuteDataReader(string sql, IStoredProcedureInput input) =>
         ExecuteSafe(input, (c, d) => c.ExecuteReader(sql, d, _transaction, commandType: CommandType.StoredProcedure, commandTimeout: c.ConnectionTimeout));

     public void Execute(string sql, IStoredProcedureInput input) =>
         ExecuteSafe(input, (c, d) => c.Execute(sql, d, _transaction, commandType: CommandType.StoredProcedure, commandTimeout: c.ConnectionTimeout));


     public TOut ExecuteSingle<TOut>(string sql, IStoredProcedureInput input) =>
         ExecuteSafe(input, (c, d) => c.QueryFirstOrDefault<TOut>(sql, d, _transaction, commandType: CommandType.StoredProcedure, commandTimeout: c.ConnectionTimeout)!);



     public IEnumerable<TOut> ExecuteList<TOut>(string sql, IStoredProcedureInput input) =>
         ExecuteSafe(input, (c, d) => c.Query<TOut>(sql, d, _transaction, commandType: CommandType.StoredProcedure, commandTimeout: c.ConnectionTimeout)!);



     public void MultiResult(string sql, IStoredProcedureInput input, Action<SqlMapper.GridReader> mapResult)
     {
         Connection ??= new SqlConnection(connStr);
         try
         {
             var d = input.ToDynamicPrarameters();
             var o = Connection.QueryMultiple(sql, d, _transaction, commandType: CommandType.StoredProcedure,
                 commandTimeout: Connection.ConnectionTimeout);
             input.UpdateOutParams(d);
             mapResult(o);
         }
         catch (Exception e)
         {
             Exception = e;
             throw e;
         }
     }

     public T MultiResult<T>(string sql, IStoredProcedureInput input, Func<SqlMapper.GridReader, T> mapResult)
     {
         Connection ??= new SqlConnection(connStr);
         try
         {
             var d = input.ToDynamicPrarameters();
             var o = Connection.QueryMultiple(sql, d, _transaction, commandType: CommandType.StoredProcedure, commandTimeout: Connection.ConnectionTimeout);
             input.UpdateOutParams(d);
             return mapResult(o);
         }
         catch (Exception e)
         {
             Exception = e;
             throw e;
         }
     }

     public IDisposable? BeginTransaction()
     {
         if (Connection == null) return null;
         if (_transaction != null) return null;

         _transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
         return _transaction;

     }

     public void CleanUp(IDisposable? tran)
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

         if (Connection.State != ConnectionState.Closed) Connection.Close();
         Connection.Dispose();
         Connection = null;
     }

     public Sp<T> GetSp<T>() where T : IStoredProcedureInput, new() => GetSp(new T());

     public Sp<T> GetSp<T>(T input) where T : IStoredProcedureInput => Sp<T>.From(input, this);

     public Sp<T> GetSp<T>(Func<T> hydrate) where T : IStoredProcedureInput => GetSp(hydrate());

     public Sp<T> GetSp<T>(Action<T> hydrate) where T : IStoredProcedureInput, new()
     {
         var sp = GetSp<T>();
         hydrate?.Invoke(sp.Input);
         return sp;
     }
 }
 /// <summary>
 /// Defines a contract for database operations, including executing SQL commands, 
 /// managing transactions, and interacting with stored procedures.
 /// </summary>
 /// <remarks>
 /// This interface provides an abstraction for database-related functionalities, 
 /// enabling the execution of SQL queries, retrieval of results, and transaction management. 
 /// It is designed to facilitate database interactions in a consistent and reusable manner.
 /// </remarks>
 public interface IDatabaseWrapper
 {
     bool WillRollBackTransaction { get; }
     bool HasTransaction { get; }
     Exception Exception { get; }
     IDbConnection? Connection { get; }
     IDisposable? BeginTransaction();

     void Execute(string sql, IStoredProcedureInput input);
     IDataReader ExecuteDataReader(string sql, IStoredProcedureInput input);
     TOut ExecuteSingle<TOut>(string sql, IStoredProcedureInput input);

     IEnumerable<TOut> ExecuteList<TOut>(string sql, IStoredProcedureInput input);
     void MultiResult(string sql, IStoredProcedureInput input, Action<SqlMapper.GridReader> mapResult);
     T MultiResult<T>(string sql, IStoredProcedureInput input, Func<SqlMapper.GridReader, T> mapResult);

     Sp<T> GetSp<T>() where T : IStoredProcedureInput, new();
     Sp<T> GetSp<T>(T input) where T : IStoredProcedureInput;
     Sp<T> GetSp<T>(Action<T> hydrate) where T : IStoredProcedureInput, new();
     Sp<T> GetSp<T>(Func<T> hydrate) where T : IStoredProcedureInput;
     void CleanUp(IDisposable transaction);
 }
 /// <summary>
 /// Defines the contract for a stored procedure with a specific input type.
 /// </summary>
 /// <typeparam name="T">
 /// The type of the input parameters for the stored procedure. This type must implement
 /// <see cref="IStoredProcedureInput"/>.
 /// </typeparam>
 /// <remarks>
 /// This interface provides methods for configuring and executing a stored procedure
 /// with strongly-typed input parameters. It also allows for setting the database wrapper
 /// and input data required for execution.
 /// </remarks>
 public interface IStoredProcedure<T> where T : IStoredProcedureInput
 {
     IDatabaseWrapper Wrapper { get; }
     T Input { get; }
     string GetSpName();
     IStoredProcedure<T> SetWrapper(IDatabaseWrapper wrapper);
     IStoredProcedure<T> SetInput(T input);
 }
 /// <summary>
 /// Indicates that a property or field is an output parameter for a stored procedure.
 /// </summary>
 /// <remarks>
 /// This attribute is used to mark properties or fields that are intended to receive
 /// output values from a stored procedure execution. It can be applied to both properties
 /// and fields within a class.
 /// </remarks>
 [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
 public class OutParameterAttribute : Attribute
 {
 }
 /// <summary>
 /// Represents metadata information about a property, including whether it is an output parameter.
 /// </summary>
 /// <remarks>
 /// This class is used to encapsulate details about a property, such as its reflection metadata
 /// and whether it is marked as an output parameter. It is primarily utilized in the context
 /// of mapping properties to stored procedure parameters.
 /// </remarks>
 internal class PInfo
 {
     public PInfo(PropertyInfo prop, bool isOut)
     {
         Prop = prop;
         IsOut = isOut;
     }

     public PropertyInfo Prop { get; }
     public bool IsOut { get; }

 }
 /// <summary>
 /// Represents a stored procedure wrapper for a specific input type.
 /// </summary>
 /// <typeparam name="TData">
 /// The type of the input data for the stored procedure. This type must implement 
 /// <see cref="IStoredProcedureInput"/>.
 /// </typeparam>
 public sealed class Sp<TData> : IStoredProcedure<TData>
     where TData : IStoredProcedureInput
 {
     private static readonly Dictionary<Type, string> _procName = new();
     /// <summary>
     /// Initializes a new instance of the <see cref="Sp{TData}"/> class.
     /// </summary>
     /// <remarks>
     /// This constructor is private to enforce the use of the <see cref="From"/> factory method for creating instances.
     /// </remarks>
     private Sp()
     {
     }

     public IDatabaseWrapper Wrapper { get; private set; }

     public TData Input { get; private set; }

     public string GetSpName()
     {
         // ReSharper disable once HeapView.PossibleBoxingAllocation
         var spType = Input.GetType();
         if (_procName.TryGetValue(spType, out var sp)) return sp;

         // check if we have a custom name specified by
         // application of StoredProcedureAttribute
         var spName = (Attribute.GetCustomAttribute(spType, typeof(StoredProcedureAttribute)) is StoredProcedureAttribute { Name: { } name })
             ? name
             : spType.Name;

         _procName.TryAdd(spType, spName);
         return spName;

     }

     public IStoredProcedure<TData> SetWrapper(IDatabaseWrapper wrapper)
     {
         Wrapper = wrapper;
         return this;
     }

     public IStoredProcedure<TData> SetInput(TData input)
     {
         Input = input;
         return this;
     }

     public static Sp<TData> From(TData input, IDatabaseWrapper wrapper)
     {
         var sp = new Sp<TData>();
         sp.SetWrapper(wrapper).SetInput(input);
         return sp;
     }

     #region SP Input Heplers

     public void Execute() => Wrapper.Execute(GetSpName(), Input);

     public IDataReader ExecuteDr() => Wrapper.ExecuteDataReader(GetSpName(), Input);

     public TOut ExecuteSingle<TOut>() => Wrapper.ExecuteSingle<TOut>(GetSpName(), Input);

     public IEnumerable<TOut> ExecuteList<TOut>() => Wrapper.ExecuteList<TOut>(GetSpName(), Input);

     public void MultiResult(Action<SqlMapper.GridReader> mapResult) => Wrapper.MultiResult(GetSpName(), Input, mapResult);

     public TOut MultiResult<TOut>(Func<SqlMapper.GridReader, TOut> mapResult) => Wrapper.MultiResult(GetSpName(), Input, mapResult);

     #endregion SP Input Heplers
 }

 public static class SqlWrapperEx
 {
     private static readonly Dictionary<Type, Dictionary<string, PInfo>> _propertyMap = new();


     private static readonly Dictionary<string, MethodInfo> _outParameterGetter = new();
     private static readonly MethodInfo _getValue = typeof(DynamicParameters).GetMethod("Get");

     private static Dictionary<string, PInfo> GetParameters<T>(this T instance) where T : IStoredProcedureInput
     {
         var type = instance.GetType();
         if (!_propertyMap.ContainsKey(type))
         {
             var props = type.GetProperties();
             var dict = new Dictionary<string, PInfo>();
             foreach (var prop in props)
             {
                 var isOut = prop.IsDefined(typeof(OutParameterAttribute), true);
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
         foreach (var (name, pInfo) in map)
         {
             var value = pInfo.Prop.GetValue(input);
             var isString = pInfo.Prop.PropertyType.Name == "String";
             var size = isString ? (int?)-1 : null;
             if (pInfo.IsOut)
                 pars.Add(name, value, direction: ParameterDirection.Output, size: size);
             else
                 pars.Add(name, value, size: size);
         }

         return pars;
     }

     public static void UpdateOutParams<T>(this T input, DynamicParameters wrapper) where T : IStoredProcedureInput
     {
         var map = input.GetParameters();
         foreach (var (name, pInfo) in map)
             if (pInfo.IsOut)
             {
                 if (!_outParameterGetter.ContainsKey(name))
                 {
                     var type = pInfo.Prop.PropertyType;
                     // ReSharper disable once HeapView.ObjectAllocation
                     var method = _getValue.MakeGenericMethod(type);
                     _outParameterGetter[name] = method;
                 }

                 object[] param = [name];
                 var value = _outParameterGetter[name].Invoke(wrapper, param);
                 pInfo.Prop.SetValue(input, value);
             }
     }

     public static IDbConnection EnsureOpen(this IDbConnection conn)
     {
         if (conn is not { State: ConnectionState.Closed }) return null;

         conn.Open();
         return conn;

     }
 }

 /// <summary>
 /// Specifies metadata for a stored procedure, including its name.
 /// </summary>
 /// <remarks>
 /// This attribute is applied to classes to associate them with a specific stored procedure.
 /// It provides a way to define the name of the stored procedure that the class represents.
 /// </remarks>
 [AttributeUsage(AttributeTargets.Class)]
 public class StoredProcedureAttribute(string name) : Attribute
 {
     public string Name { get; init; } = name;
 }
    
    
    
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit
    {
    }
}
