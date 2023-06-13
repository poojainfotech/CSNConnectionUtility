using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CSNConnectionUtility
{
    public interface IDataBaseUtility
    {
        SqlConnection GetDatBaseConnection();
        void RunStoredProcedure(Queue parameter, String storedProcedure);
        String RunStoredProcedureForXmlString(Queue parameter, String storedProcedure);

        SqlDataReader RunStoredProcedureForDataReader(Queue parameter, String storedProcedure);

        SqlDataReader RunStoredProcedureForDataReader(String storedProcedure);

        IDataReader RunStoredProcedureForIDataReader(String storedProcedure);

        void CommitTransation(bool blnCommit);

        void RollBackTransation();

        void ClosedConnection();

        void RunNonQuery(String storedProcedure);

        string RunExecuteScaler(String storedProcedure);

        DataSet GetDataSet(String storedProcedure);

        DataSet GetDataSet(String storedProcedure, string tableName);

        DataSet GetDataSet(String storedProcedure, string tableName, Queue paramName, bool isDataSet);

        DataTable GetDataSet(String storedProcedure, string tableName, Queue paramName);

        DataSet GetDataSet(String storedProcedure, Queue paramName);

        void CreateTransactionCommand();

        void SetCommandHighTimeOut(SqlCommand sqlCommand);

        void CancelCommad();

        void GetDataSet(string sql, string tableName, Queue paramQueue, ref DataSet refDataSet);

        void ReadTimeOut();

        XmlTextReader RunStoredProcedureForXmlTextReader(string storedProcedure, Queue paramQueue);

        SqlDataReader RunStoredProcedureForTransation(string storedProcedure, Queue paramQueue);

        int InsertDataInTable(string sqlQuery, Queue paramName);

        int UserSession(string sqlQuery, Queue paramName);

        int GetNumberOfActiveUser(string sqlQuery, Queue paramName);
    }
    public class DataBaseUtility : IDataBaseUtility
    {
        private SqlConnection _sqlConnection;
        private SqlCommand _sqlCommand;
        private string _connectionString;
        private SqlTransaction _sqlTransaction;
        private int _executionTimeOut;


        public DataBaseUtility()
        {
            try
            {
                //this._connectionString = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
                //this._connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CSNDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                this._connectionString = @"Data Source=LAPTOP-POMJSQ9S\SQLEXPRESS;Database=CSNDatabase; user id=sa; password=pooja@123;";

                //this._connectionString = @"Data Source=LAPTOP-POMJSQ9S\SQLEXPRESS;Database=CSNDatabase; Trusted_Connection = True;";

                this._sqlConnection = new SqlConnection(this._connectionString);
                this._sqlCommand = new SqlCommand();
                this._sqlConnection.Open();
                this.ReadTimeOut();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public int ExecutionTimeOut
        {
            get
            {
                return this._executionTimeOut;
            }
            set
            {
                this._executionTimeOut = value;
                if (this._sqlCommand != null)
                {
                    this._sqlCommand.CommandTimeout = value;
                }
            }
        }

        public string GetConnectionString()
        {
            try
            {
                return this._connectionString;
            }
            catch (Exception)
            {
            }
            return "";
        }

        public void ClosedConnection()
        {
            if (this._sqlConnection.State == ConnectionState.Open)
            {
                this._sqlConnection.Close();
            }
        }

        public void CommitTransation(bool blnCommit)
        {
            try
            {
                this._sqlTransaction.Commit();
                if (blnCommit)
                {
                    this._sqlConnection.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        public void CreateTransactionCommand()
        {
            this._sqlCommand = new SqlCommand();
            this.ReadTimeOut();
            this._sqlCommand.CommandTimeout = this._executionTimeOut;
            this._sqlTransaction = this._sqlConnection.BeginTransaction();
            this._sqlCommand.Transaction = this._sqlTransaction;
        }

        private void ReadTimeOut()
        {
            if (ConfigurationManager.AppSettings["ExecutionTimeOut"] == null)
            {
                _executionTimeOut = 6000; //Convert.ToInt32(ConfigurationManager.AppSettings["ExecutionTimeOut"]);
            }
            else
            {
                _executionTimeOut = 6000;
                //throw new Exception("Execution Time Out Is Not Set In Config file.");

            }
        }

        public DataSet GetDataSet(string sqlQuery)
        {
            try
            {
                DataSet ds = new DataSet();
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sda = new SqlDataAdapter(sqlQuery, this._sqlConnection);
                sda.Fill(ds);
                this._sqlConnection.Close();
                return ds;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataSet GetDataSet(string storedProcedure, string tableName)
        {
            try
            {
                DataSet ds = new DataSet();
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sda = new SqlDataAdapter(storedProcedure, this._sqlConnection);
                sda.Fill(ds, tableName);
                this._sqlConnection.Close();
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetDataSet(string storedProcedure, string tableName, Queue paramName)
         {
            DataSet dataSet = null;
            try
            {
                if (this._sqlConnection.State == ConnectionState.Closed)
                {
                    this._sqlConnection.Open();
                }
                if (this._sqlCommand != null)
                {
                    this._sqlCommand = new SqlCommand();
                }
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.CommandText = storedProcedure;
                this._sqlCommand.Connection = this._sqlConnection;

                this._sqlCommand.CommandTimeout = this._executionTimeOut;
                if(paramName != null)
                {
                    while (paramName.Count > 0)
                    {
                        this._sqlCommand.Parameters.Add((SqlParameter)paramName.Dequeue());
                    }
                }
                dataSet = new DataSet();
                SqlDataAdapter sda = new SqlDataAdapter(this._sqlCommand);
                sda.Fill(dataSet, tableName);
                this._sqlConnection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dataSet.Tables[tableName];
        }

        public int InsertDataInTable(string sqlQuery, Queue paramName)
        {
            try
            {
                if (this._sqlConnection.State == ConnectionState.Closed)
                {
                    this._sqlConnection.Open();
                }
                if (this._sqlCommand != null)
                {
                    this._sqlCommand = new SqlCommand();
                }
                int resultStatus = 0;
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.CommandText = sqlQuery;
                this._sqlCommand.Connection = this._sqlConnection;

                this._sqlCommand.CommandTimeout = this._executionTimeOut;
                while (paramName.Count > 0)
                {
                    this._sqlCommand.Parameters.Add((SqlParameter)paramName.Dequeue());
                }
                resultStatus = this._sqlCommand.ExecuteNonQuery();
                this._sqlConnection.Close();
                return resultStatus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataSet GetDataSet(string storedProcedure, Queue paramName)
        {
            DataSet dataSet = null;
            try
            {
                if (this._sqlConnection.State == ConnectionState.Closed)
                {
                    this._sqlConnection.Open();
                }
                if (this._sqlCommand != null)
                {
                    this._sqlCommand = new SqlCommand();
                }
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.CommandText = storedProcedure;
                this._sqlCommand.Connection = this._sqlConnection;

                this._sqlCommand.CommandTimeout = this._executionTimeOut;
                while (paramName.Count > 0)
                {
                    this._sqlCommand.Parameters.Add((SqlParameter)paramName.Dequeue());
                }
                dataSet = new DataSet();
                SqlDataAdapter sda = new SqlDataAdapter(this._sqlCommand);
                sda.Fill(dataSet);
                this._sqlConnection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dataSet;
        }

        public SqlConnection GetDatBaseConnection()
        {
            this._sqlConnection = new SqlConnection(this._connectionString);
            this._sqlConnection.Open();
            return this._sqlConnection;
        }

        public void RollBackTransation()
        {
            try
            {
                if (this._sqlTransaction != null)
                {
                    if (this._sqlTransaction.Connection != null)
                    {
                        this._sqlTransaction.Rollback();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public string RunExecuteScaler(string storedProcedure)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                return objSqlCommand.ExecuteScalar().ToString();
            }
            catch (Exception)
            {

            }
            return "";
        }

        public void RunNonQuery(string storedProcedure)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                objSqlCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
        }

        public void RunStoredProcedure(Queue parameter, string storedProcedure)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                objSqlCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {

            }
        }

        public SqlDataReader RunStoredProcedureForDataReader(Queue parameter, string storedProcedure)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                while (parameter.Count > 0)
                {
                    objSqlCommand.Parameters.Add((SqlParameter)(parameter.Dequeue()));
                }
                return objSqlCommand.ExecuteReader();
            }
            catch (Exception)
            {

            }
            return null;
        }

        public SqlDataReader RunStoredProcedureForDataReader(string storedProcedure)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                return objSqlCommand.ExecuteReader();
            }
            catch (Exception)
            {

            }
            return null;
        }

        public IDataReader RunStoredProcedureForIDataReader(string storedProcedure)
        {
            return RunStoredProcedureForDataReader(storedProcedure);
        }

        public string RunStoredProcedureForXmlString(Queue parameter, string storedProcedure)
        {
            SqlCommand objSqlCommand = null;
            XmlReader xmlReader;
            StringBuilder objStringBuilder = new StringBuilder();
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                while (parameter.Count > 0)
                {
                    objSqlCommand.Parameters.Add((SqlParameter)(parameter.Dequeue()));
                }
                xmlReader = objSqlCommand.ExecuteXmlReader();
                while (!xmlReader.EOF)
                {
                    objStringBuilder.Append(xmlReader.ReadOuterXml());
                }
                xmlReader.Close();

            }
            catch (Exception)
            {

            }
            return objStringBuilder.ToString();
        }

        public void SetCommandHighTimeOut(SqlCommand sqlCommand)
        {
            sqlCommand.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ExecutionTimeOut"]);
        }

        public void CancelCommad()
        {
            this._sqlCommand.Cancel();
        }

        public void GetDataSet(string sql, string tableName, Queue paramQueue, ref DataSet refDataSet)
        {
            SqlCommand sqlCommand = null;
            SqlDataAdapter sqlDataAdapter = null;
            try
            {
                sqlCommand = new SqlCommand(sql, this._sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = this._executionTimeOut;
                while (paramQueue.Count > 0)
                {
                    sqlCommand.Parameters.Add((SqlParameter)paramQueue.Dequeue());
                }
                sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(refDataSet, tableName);

            }
            catch (Exception)
            {

            }
        }

        void IDataBaseUtility.ReadTimeOut()
        {
            if (ConfigurationManager.AppSettings["ExecutionTimeOut"] == null)
            {
                _executionTimeOut = 6000; //Convert.ToInt32(ConfigurationManager.AppSettings["ExecutionTimeOut"]);
            }
            else
            {
                _executionTimeOut = 6000; //throw new Exception("Execution Time Out Is Not Set In Config file.");
            }
        }

        public XmlTextReader RunStoredProcedureForXmlTextReader(string storedProcedure, Queue paramQueue)
        {
            SqlCommand objSqlCommand = null;
            try
            {
                objSqlCommand = new SqlCommand(storedProcedure, this._sqlConnection);
                objSqlCommand.CommandTimeout = this._executionTimeOut;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                while (paramQueue.Count > 0)
                {
                    objSqlCommand.Parameters.Add((SqlParameter)(paramQueue.Dequeue()));
                }
                return (XmlTextReader)objSqlCommand.ExecuteXmlReader();
            }
            catch (Exception)
            {

            }
            return null;
        }

        public SqlDataReader RunStoredProcedureForTransation(string storedProcedure, Queue paramQueue)
        {
            try
            {
                this._sqlCommand.CommandText = storedProcedure;
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.Parameters.Clear();
                while (paramQueue.Count > 0)
                {
                    this._sqlCommand.Parameters.Add((SqlParameter)(paramQueue.Dequeue()));
                }
                return this._sqlCommand.ExecuteReader();
            }
            catch (Exception)
            {
                this._sqlTransaction.Rollback();
            }
            return null;
        }

        public int UserSession(string sqlQuery, Queue paramName)
        {
            try
            {
                if (this._sqlConnection.State == ConnectionState.Closed)
                {
                    this._sqlConnection.Open();
                }
                if(this._sqlCommand != null)
                {
                    this._sqlCommand = new SqlCommand();
                }
                int resultStatus = 0;
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.CommandText = sqlQuery;
                this._sqlCommand.Connection = this._sqlConnection;

                this._sqlCommand.CommandTimeout = this._executionTimeOut;
                while (paramName.Count > 0)
                {
                    this._sqlCommand.Parameters.Add((SqlParameter)paramName.Dequeue());
                }
                resultStatus = this._sqlCommand.ExecuteNonQuery();
                this._sqlCommand = null;
                this._sqlConnection.Close();
                return resultStatus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int GetNumberOfActiveUser(string storedProcedure, Queue paramName)
        {
            DataSet dataSet = null;
            int iNumberActiveUser = 0;
            try
            {
                if (this._sqlConnection.State == ConnectionState.Closed)
                {
                    this._sqlConnection.Open();
                }
                if (this._sqlCommand != null)
                {
                    this._sqlCommand = new SqlCommand();
                }
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.CommandText = storedProcedure;
                this._sqlCommand.Connection = this._sqlConnection;

                this._sqlCommand.CommandTimeout = this._executionTimeOut;
                if(paramName != null)
                {
                    while (paramName.Count > 0)
                    {
                        this._sqlCommand.Parameters.Add((SqlParameter)paramName.Dequeue());
                    }
                }
                
                dataSet = new DataSet();
                SqlDataAdapter sda = new SqlDataAdapter(this._sqlCommand);
                sda.Fill(dataSet);

                if(dataSet != null)
                {
                    iNumberActiveUser = dataSet.Tables[0].Rows.Count;
                }

                this._sqlConnection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return iNumberActiveUser;
        }

        public DataSet GetDataSet(string storedProcedure, string tableName, Queue paramName, bool isDataSet)
        {
            try
            {
                DataSet ds = new DataSet();
                this._sqlCommand.CommandType = CommandType.StoredProcedure;
                this._sqlCommand.CommandText = storedProcedure;
                this._sqlCommand.Connection = this._sqlConnection;

                this._sqlCommand.CommandTimeout = this._executionTimeOut;
                if (paramName != null)
                {
                    while (paramName.Count > 0)
                    {
                        this._sqlCommand.Parameters.Add((SqlParameter)paramName.Dequeue());
                    }
                }
                SqlDataAdapter sda = new SqlDataAdapter(this._sqlCommand);
                sda.Fill(ds, tableName);
                this._sqlConnection.Close();
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
