using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using RSToolKit.Domain;
using RSToolKit.Domain.Errors;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using RSToolKit.Domain.Entities.Clients;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// The SqlHelper holds information that makes grabbing the desired connections strings easier.
    /// </summary>
    public static class SqlHelper
    {

        private static string _sqlServer = "";
        private static string _userId = "";
        private static string _password = "";
        private static string _initialCatalog = "";

        public static void SetValues(string server, string userId, string password, string initialCatalog)
        {
            _sqlServer = server;
            _userId = userId;
            _password = password;
            _initialCatalog = initialCatalog;
        }

        public static void SetValues(SqlConnectionStringBuilder connectionString)
        {
            _sqlServer = connectionString.DataSource;
            _userId = connectionString.UserID;
            _password = connectionString.Password;
            _initialCatalog = connectionString.InitialCatalog;
        }

        /// <summary>
        /// Gets the "RegStepConnectionString" from web.config and wraps it in the System.Data.SqlClient.SqlConnectionStringBuilder.
        /// </summary>
        /// <returns>Returns a System.Data.SqlClient.SqlConnectionStringBuilder with the value of the connection string.</returns>
        public static SqlConnectionStringBuilder GetConnectionString()
        {
            SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
            connString.DataSource = _sqlServer;
            connString.UserID = _userId;
            connString.Password = _password;
            connString.InitialCatalog = _initialCatalog;
            return connString;
        }

        /// <summary>
        /// Gets the connection string for the desired company.
        /// </summary>
        /// <param name="company">The Guid of the company.</param>
        /// <returns>Returns a System.Data.SqlClient.SqlConnectionStringBuilder with the value of the connection string.Returns null if the company does not exist</returns>
        public static SqlConnectionStringBuilder GetConnectionString(Guid company)
        {
            SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
            connString.DataSource = _sqlServer;
            connString.UserID = _userId;
            connString.Password = _password;
            connString.InitialCatalog = GetCompanyDatabase(company);
            if (connString.InitialCatalog == "")
            {
                return null;
            }
            return connString;
        }

        /// <summary>
        /// Gets the database that holds a companies information.
        /// </summary>
        /// <param name="CompanyId">The unique id of the company that represents the company.</param>
        /// <returns>Returns the database name or null if the company does not exist.</returns>
        public static string GetCompanyDatabase(Guid companyId)
        {
            using (var context = new EFDbContext())
            {
                var company = context.Companies.FirstOrDefault(c => c.UId == companyId);
                if (company == null)
                    return null;
                return null;
            }
        }


        /// <summary>
        /// Gets the connection string for the email information.
        /// </summary>
        /// <returns>Returns a System.Data.SqlClient.SqlConnectionStringBuilder with the value of the connection string.</returns>
        public static SqlConnectionStringBuilder GetRegStepEmailConnection()
        {
            SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
            connString.DataSource = _sqlServer;
            connString.UserID = _userId;
            connString.Password = _password;
            connString.InitialCatalog = "RegStepEmail";
            return connString;
        }

        /// <summary>
        /// Gets the connection string for the credit card information.
        /// </summary>
        /// <returns>Returns a System.Data.SqlClient.SqlConnectionStringBuilder with the value of the connection string.</returns>
        public static SqlConnectionStringBuilder GetRegStepCreditCardConnection()
        {
            SqlConnectionStringBuilder connString = new SqlConnectionStringBuilder();
            connString.DataSource = _sqlServer;
            connString.UserID = _userId;
            connString.Password = _password;
            connString.InitialCatalog = "RegStepCC";
            return connString;
        }

        public static string GetSqlFilter(List<FilterInformation> filters, SqlCommand cmd)
        {
            string filter = "1=1";
            bool groupOpen = false;
            if (filters != null && filters.Count > 0)
            {
                filter = "";
                int fCount = -1;
                foreach (var f in filters)
                {
                    fCount++;
                    if (!groupOpen)
                    {
                        filter += "(";
                        groupOpen = true;
                    }
                    filter += "[" + f.ActingOn + "] ";
                    switch (f.Test)
                    {
                        case LogicTest.Equal:
                            filter += "= ";
                            break;
                        case LogicTest.GreaterThan:
                            filter += "> ";
                            break;
                        case LogicTest.GreaterThanOrEqual:
                            filter += ">= ";
                            break;
                        case LogicTest.In:
                            filter += "IN (";
                            break;
                        case LogicTest.LessThan:
                            filter += "< ";
                            break;
                        case LogicTest.LessThanOrEqual:
                            filter += "<= ";
                            break;
                        case LogicTest.NotEqual:
                            filter += "<> ";
                            break;
                        case LogicTest.NotIn:
                            filter += "NOT IN (";
                            break;
                        case LogicTest.StartsWith:
                        case LogicTest.Contains:
                        case LogicTest.EndsWith:
                            filter += "LIKE ";
                            break;
                    }
                    if (f.Test != LogicTest.In && f.Test != LogicTest.NotIn)
                    {
                        filter += "@filter" + fCount;
                        DateTime date;
                        int numberInt;
                        decimal numberDecimal;
                        double numberDouble;
                        long numberLong;
                        if (DateTime.TryParse(f.Value, out date))
                        {
                            cmd.Parameters.Add("@filter" + fCount, SqlDbType.DateTime).Value = date;
                        }
                        else if (Int32.TryParse(f.Value, out numberInt))
                        {
                            cmd.Parameters.Add("@filter" + fCount, SqlDbType.Int).Value = numberInt;
                        }
                        else if (Decimal.TryParse(f.Value, out numberDecimal))
                        {
                            cmd.Parameters.Add("@filter" + fCount, SqlDbType.Decimal).Value = numberDecimal;
                        }
                        else if (Double.TryParse(f.Value, out numberDouble))
                        {
                            cmd.Parameters.AddWithValue("@filter" + fCount, numberDouble).Value = numberDouble;
                        }
                        else if (long.TryParse(f.Value, out numberLong))
                        {
                            cmd.Parameters.Add("@filter" + fCount, SqlDbType.BigInt).Value = numberLong;
                        }
                        else
                        {
                            switch (f.Test)
                            {
                                case LogicTest.StartsWith:
                                    f.Value += "%";
                                    break;
                                case LogicTest.Contains:
                                    f.Value = "%" + f.Value + "%";
                                    break;
                                case LogicTest.EndsWith:
                                    f.Value = "%" + f.Value;
                                    break;
                            }
                            cmd.Parameters.Add("@filter" + fCount, SqlDbType.VarChar).Value = f.Value;
                        }
                    }
                    else
                    {
                        string value = f.Value;
                        value = Regex.Replace(value, @";", "");
                        value = Regex.Replace(value, @"'", "");
                        value = Regex.Replace(value, @", ", ",");
                        var values = value.Split(',');
                        foreach (var s in values)
                        {
                            filter += "'" + s + "',";
                        }
                        filter = filter.Remove(filter.Length - 1, 1);
                        filter += ")";
                    }
                    if (!f.GroupNext)
                    {
                        filter += ")";
                        groupOpen = false;
                    }
                    if (f.Link != FilterLink.None)
                    {
                        switch (f.Link)
                        {
                            case FilterLink.And:
                                filter += " AND ";
                                break;
                            case FilterLink.Or:
                                filter += " OR ";
                                break;
                        }
                    }
                }
            }
            return filter;
        }

        public static string GetSqlOrder(List<SortingInformation> sortings)
        {
            string order = "[Id] ASC";
            if (sortings != null && sortings.Count > 0)
            {
                order = "";
                sortings.Sort();
                foreach (var s in sortings)
                {
                    order += "[" + s.ActingOn + "] " + s.Type + ", ";
                }
                order = order.Remove(order.Length - 2, 2);
            }
            return order;
        }

    }

    public class CompanyInformation
    {
        public Guid Id { get; set; }
        public string Database { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

}