﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DataAccess.Other;
using DataTransferObjects;
using DataTransferObjects.CornerStone;

namespace DataAccess
{
    public static class ConfigurationDAL
    {
        #region Constants

        #region Parameters

        private const string TitleParam = "@Title";
        private const string ImageNameParam = "@FileName";
        private const string ImagePathParam = "@FilePath";
        private const string DataParam = "@Data";
        private const string GoalTypeIdParam = "@GoalTypeId";
        private const string ColorIdParam = "@ColorId";
        private const string StartRangeParam = "@StartRange";
        private const string EndRangeParam = "@EndRange";
        private const string isDeletePreviousMarginInfoParam = "@isDeletePreviousMarginInfo";
        private const string isNotesRequiredPracticeIdsListParam = "@NotesRequiredPracticeIdsList";
        private const string isNotesExemptedPracticeIdsListParam = "@NotesExemptedPracticeIdsList";
        private const string linkNameListParam = "@linkNameList";
        private const string virtualPathListParam = "@virtualPathList";
        private const string dashBoardTypeParam = "@dashBoardType";

        #endregion Parameters

        #region Columns

        private const string TitleColumn = "Title";
        private const string FileNameColumn = "FileName";
        private const string FilePathColumn = "FilePath";
        private const string DataColumn = "Data";

        #endregion Columns

        #endregion Constants

        public static void SaveCompanyLogoData(string title, string imagename, string imagePath, Byte[] data)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.CompanyLogoDataSaveProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(TitleParam, title);
                    command.Parameters.AddWithValue(ImageNameParam, !string.IsNullOrEmpty(imagename) ? (object)imagename : DBNull.Value);
                    command.Parameters.AddWithValue(ImagePathParam, !string.IsNullOrEmpty(imagePath) ? (object)imagePath : DBNull.Value);
                    command.Parameters.Add(DataParam, SqlDbType.VarBinary, -1);
                    command.Parameters[DataParam].Value = data != null ? (object)data : DBNull.Value;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static CompanyLogo GetCompanyLogoData()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetCompanyLogoDataProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        CompanyLogo companylogo = new CompanyLogo();
                        if (reader.HasRows)
                        {
                            int titleIndex = reader.GetOrdinal(TitleColumn);
                            int FileNameIndex = reader.GetOrdinal(FileNameColumn);
                            int FilePathIndex = reader.GetOrdinal(FilePathColumn);
                            int DataColumnIndex = reader.GetOrdinal(DataColumn);

                            while (reader.Read())
                            {
                                companylogo.Title = reader.GetString(titleIndex);
                                companylogo.FileName = !reader.IsDBNull(FileNameIndex) ? reader.GetString(FileNameIndex) : string.Empty;
                                companylogo.FilePath = !reader.IsDBNull(FilePathIndex) ? reader.GetString(FilePathIndex) : string.Empty;
                                companylogo.Data = (byte[])reader[DataColumnIndex];
                            }
                        }
                        return companylogo;
                    }
                }
            }
        }

        public static string GetCompanyName()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetCompanyNameProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    connection.Open();
                    return (string)command.ExecuteScalar();
                }
            }
        }

        public static void SaveResourceKeyValuePairs(SettingsType settingType, Dictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Keys.Count <= 0) return;
            foreach (var item in dictionary)
            {
                using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
                {
                    using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveSettingsKeyValuePairsProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = connection.ConnectionTimeout;

                        command.Parameters.AddWithValue(Constants.ParameterNames.SettingsTypeParam, (int)settingType);
                        command.Parameters.AddWithValue(Constants.ParameterNames.SettingsKeyParam, item.Key);
                        command.Parameters.AddWithValue(Constants.ParameterNames.ValueParam, item.Value);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static bool SaveResourceKeyValuePairItem(SettingsType settingType, string key, string value, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            int rowsAffected = 0;

            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveSettingsKeyValuePairsProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.SettingsTypeParam, (int)settingType);
                command.Parameters.AddWithValue(Constants.ParameterNames.SettingsKeyParam, key);
                command.Parameters.AddWithValue(Constants.ParameterNames.ValueParam, value);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (activeTransaction != null)
                {
                    command.Transaction = activeTransaction;
                }

                rowsAffected = command.ExecuteNonQuery();
            }

            return rowsAffected > 0;
        }

        public static Dictionary<string, string> GetResourceKeyValuePairs(SettingsType settingType)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetSettingsKeyValuePairsProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(Constants.ParameterNames.SettingsTypeParam, (int)settingType);
                    command.CommandTimeout = connection.ConnectionTimeout;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();

                        if (reader.HasRows)
                        {
                            int ResourcesKeyIndex = reader.GetOrdinal(Constants.ColumnNames.SettingsKeyColumn);
                            int ValueIndex = reader.GetOrdinal(Constants.ColumnNames.ValueColumn);

                            while (reader.Read())
                            {
                                dictionary.Add(reader.GetString(ResourcesKeyIndex), reader.GetString(ValueIndex));
                            }
                        }

                        return dictionary;
                    }
                }
            }
        }

        public static void SaveMarginInfoDetail(List<Triple<DefaultGoalType, Triple<SettingsType, string, string>, List<ClientMarginColorInfo>>> marginInfoList)
        {
            if (marginInfoList == null || marginInfoList.Count <= 0) return;
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                foreach (var triple in marginInfoList)
                {
                    SaveResourceKeyValuePairItem((SettingsType)triple.Second.First, triple.Second.Second, triple.Second.Third);

                    if (!Convert.ToBoolean(triple.Second.Third) || triple.Third == null) continue;
                    for (int i = 0; i < triple.Third.Count; i++)
                    {
                        bool isDeletePreviousMarginInfo = i == 0;
                        DefaultMarginColorInfoInsert((int)triple.First, triple.Third[i], isDeletePreviousMarginInfo, connection, transaction);
                    }
                }

                transaction.Commit();
            }
        }

        private static void DefaultMarginColorInfoInsert(int goalTypeId, ClientMarginColorInfo marginColorInfo, bool isDeletePreviousMarginInfo, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveMarginInfoDefaultsProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(GoalTypeIdParam, goalTypeId);
                command.Parameters.AddWithValue(ColorIdParam, marginColorInfo.ColorInfo.ColorId);
                command.Parameters.AddWithValue(StartRangeParam, marginColorInfo.StartRange);
                command.Parameters.AddWithValue(EndRangeParam, marginColorInfo.EndRange);
                command.Parameters.AddWithValue(isDeletePreviousMarginInfoParam, isDeletePreviousMarginInfo);

                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    if (activeTransaction != null)
                    {
                        command.Transaction = activeTransaction;
                    }

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static List<ClientMarginColorInfo> GetMarginColorInfoDefaults(DefaultGoalType goalType)
        {
            var clientMarginColorInfo = new List<ClientMarginColorInfo>();
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetMarginColorInfoDefaultsProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    command.Parameters.AddWithValue(GoalTypeIdParam, (int)goalType);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ReadMarginColorInfoDefaults(reader, clientMarginColorInfo);
                    }
                }
            }
            return clientMarginColorInfo.Count > 0 ? clientMarginColorInfo : null;
        }

        private static void ReadMarginColorInfoDefaults(SqlDataReader reader, List<ClientMarginColorInfo> clientMarginColorInfo)
        {
            if (!reader.HasRows) return;
            int colorIdIndex = reader.GetOrdinal(Constants.ColumnNames.ColorIdColumn);
            int colorValueIndex = reader.GetOrdinal(Constants.ColumnNames.ValueColumn);
            int startRangeIndex = reader.GetOrdinal(Constants.ColumnNames.StartRangeColumn);
            int endRangeIndex = reader.GetOrdinal(Constants.ColumnNames.EndRangeColumn);
            int colorDescriptionIndex = reader.GetOrdinal(Constants.ColumnNames.DescriptionColumn);

            while (reader.Read())
            {
                clientMarginColorInfo.Add(
                    new ClientMarginColorInfo()
                        {
                            ColorInfo = new ColorInformation()
                                {
                                    ColorId = reader.GetInt32(colorIdIndex),
                                    ColorValue = reader.GetString(colorValueIndex),
                                    ColorDescription = reader.GetString(colorDescriptionIndex)
                                },
                            StartRange = reader.GetInt32(startRangeIndex),
                            EndRange = reader.GetInt32(endRangeIndex),
                        }
                    );
            }
        }

        public static void SavePracticesIsNotesRequiredDetails(string isNotesRequiredPracticeIdsList, string isNotesExemptedPracticeIdsList)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SavePracticesIsNotesRequiredDetailsProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(isNotesRequiredPracticeIdsListParam, isNotesRequiredPracticeIdsList);
                    command.Parameters.AddWithValue(isNotesExemptedPracticeIdsListParam, isNotesExemptedPracticeIdsList);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void SaveQuickLinksForDashBoard(string linkNameList, string virtualPathList, DashBoardType dashBoardType)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveQuickLinksForDashBoardProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(linkNameListParam, linkNameList);
                    command.Parameters.AddWithValue(virtualPathListParam, virtualPathList);
                    command.Parameters.AddWithValue(dashBoardTypeParam, ((int)dashBoardType));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<QuickLinks> GetQuickLinksByDashBoardType(DashBoardType dashBoardtype)
        {
            var quicklinks = new List<QuickLinks>();
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetQuickLinksByDashBoardTypeProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    command.Parameters.AddWithValue(dashBoardTypeParam, (int)dashBoardtype);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ReadQuickLinks(reader, quicklinks);
                    }
                }
            }
            return quicklinks;
        }

        private static void ReadQuickLinks(SqlDataReader reader, List<QuickLinks> quicklinks)
        {
            if (!reader.HasRows) return;
            int dashBoardTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.DashBoardTypeIdColumn);
            int idIndex = reader.GetOrdinal(Constants.ColumnNames.Id);
            int linkNameIndex = reader.GetOrdinal(Constants.ColumnNames.LinkNameColumn);
            int virtualPathIndex = reader.GetOrdinal(Constants.ColumnNames.VirtualPathColumn);

            while (reader.Read())
            {
                quicklinks.Add(new QuickLinks()
                    {
                        DashBoardType = (DashBoardType)reader.GetInt32(dashBoardTypeIdIndex),
                        Id = reader.GetInt32(idIndex),
                        LinkName = reader.GetString(linkNameIndex),
                        VirtualPath = reader.GetString(virtualPathIndex)
                    });
            }
        }

        public static void DeleteQuickLinkById(int id)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.DeleteQuickLinkByIdProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.Id, id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void SaveAnnouncement(string text, string richText)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveAnnouncement, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.TextParam, text);
                    command.Parameters.AddWithValue(Constants.ParameterNames.RichTextParam, richText);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static string GetLatestAnnouncement()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetLatestAnnouncement, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    connection.Open();
                    return (string)command.ExecuteScalar();
                }
            }
        }

        public static List<string> GetAllDomains()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetAllDomainsProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var result = new List<string>();
                    if (reader.HasRows)
                    {
                        int nameIndex = reader.GetOrdinal(Constants.ColumnNames.Name);
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(nameIndex));
                        }
                    }
                    return result;
                }
            }
        }

        #region RecruitingMetrics

        public static List<RecruitingMetrics> GetRecruitingMetrics(int? recruitingMetricsTypeId)
        {
            var recruitingMetrics = new List<RecruitingMetrics>();
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetRecruitingMetrics, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    command.Parameters.AddWithValue(Constants.ParameterNames.RecruitingMetricsTypeId, recruitingMetricsTypeId.HasValue ? recruitingMetricsTypeId.Value : (object)DBNull.Value);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ReadRecruitingMetrics(reader, recruitingMetrics);
                    }
                }
            }
            return recruitingMetrics;
        }

        private static void ReadRecruitingMetrics(SqlDataReader reader, List<RecruitingMetrics> recruitingMetrics)
        {
            if (!reader.HasRows) return;
            int recruitingMetricsIdndex = reader.GetOrdinal(Constants.ColumnNames.RecruitingMetricsId);
            int nameIndex = reader.GetOrdinal(Constants.ColumnNames.RecruitingMetrics);
            int recruitingMetricsTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.RecruitingMetricsTypeId);
            int sortOrderIndex = reader.GetOrdinal(Constants.ColumnNames.SortOrder);
            int recruitingMetricsInUseIndex = reader.GetOrdinal(Constants.ColumnNames.RecruitingMetricsInUse);

            while (reader.Read())
            {
                recruitingMetrics.Add(new RecruitingMetrics()
                {
                    RecruitingMetricsType = (RecruitingMetricsType)reader.GetInt32(recruitingMetricsTypeIdIndex),
                    RecruitingMetricsId = reader.GetInt32(recruitingMetricsIdndex),
                    Name = reader.GetString(nameIndex),
                    SortOrder = reader.GetInt32(sortOrderIndex),
                    InUse = reader.GetBoolean(recruitingMetricsInUseIndex)
                });
            }
        }

        public static void SaveRecruitingMetrics(RecruitingMetrics metric)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveRecruitingMetrics, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.RecruitingMetricsId, metric.RecruitingMetricsId);
                    command.Parameters.AddWithValue(Constants.ParameterNames.Name, metric.Name);
                    command.Parameters.AddWithValue(Constants.ParameterNames.SortOrder, metric.SortOrder);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void RecruitingMetricsDelete(int recruitingMetricId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.RecruitingMetricsDelete, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue(Constants.ParameterNames.RecruitingMetricsId, recruitingMetricId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void RecruitingMetricsInsert(RecruitingMetrics metrics)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.RecruitingMetricsInsert, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue(Constants.ParameterNames.Name, metrics.Name);
                command.Parameters.AddWithValue(Constants.ParameterNames.RecruitingMetricsTypeId, (int)metrics.RecruitingMetricsType);
                command.Parameters.AddWithValue(Constants.ParameterNames.SortOrder, metrics.SortOrder);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Lockout

        public static List<Lockout> GetLockoutDetails(int? lockoutPageId)
        {
            var lockouts = new List<Lockout>();
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetLockoutDetails, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    command.Parameters.AddWithValue(Constants.ParameterNames.LockoutPageId, lockoutPageId.HasValue ? lockoutPageId.Value : (object)DBNull.Value);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ReadLockoutDetails(reader, lockouts);
                    }
                }
            }
            return lockouts;
        }

        private static void ReadLockoutDetails(SqlDataReader reader, List<Lockout> lockouts)
        {
            if (!reader.HasRows) return;
            int lockoutIdIndex = reader.GetOrdinal(Constants.ColumnNames.LockoutId);
            int lockoutPageIdIndex = reader.GetOrdinal(Constants.ColumnNames.LockoutPageId);
            int functionalityNameIndex = reader.GetOrdinal(Constants.ColumnNames.FunctionalityName);
            int lockoutIndex = reader.GetOrdinal(Constants.ColumnNames.Lockout);
            int lockoutDateIndex = reader.GetOrdinal(Constants.ColumnNames.LockoutDate);
            while (reader.Read())
            {
                lockouts.Add(new Lockout()
                {
                    Id = reader.GetInt32(lockoutIdIndex),
                    Name = reader.GetString(functionalityNameIndex),
                    LockoutPage = (LockoutPages)reader.GetInt32(lockoutPageIdIndex),
                    IsLockout = reader.GetBoolean(lockoutIndex),
                    LockoutDate = reader.IsDBNull(lockoutDateIndex) ? null : (DateTime?)reader.GetDateTime(lockoutDateIndex)
                });
            }
        }

        public static void SaveLockoutDetails(string lockoutXML)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.SaveLockoutDetails, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;
                command.Parameters.AddWithValue(Constants.ParameterNames.LockoutXML, lockoutXML);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        #endregion

        public static List<DivisionCF> GetCFDivisions()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                return GetCFDivisions(connection);
            }
        }

        public static List<DivisionCF> GetCFDivisions(SqlConnection connection)
        {
            var cFDivisions = new List<DivisionCF>();

            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetCFDivisions, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadCFDivisions(reader, cFDivisions);
                }
            }
            return cFDivisions;
        }

        private static void ReadCFDivisions(SqlDataReader reader, List<DivisionCF> cFDivisions)
        {
            if (!reader.HasRows) return;
            int divisionIdndex = reader.GetOrdinal(Constants.ColumnNames.DivisionId);
            int divisionCodeIndex = reader.GetOrdinal(Constants.ColumnNames.DivisionCode);
            int divisionNameIndex = reader.GetOrdinal(Constants.ColumnNames.DivisionName);
            int parentIdIndex = reader.GetOrdinal(Constants.ColumnNames.ParentId);
            int parentDivisionCodeIndex = reader.GetOrdinal(Constants.ColumnNames.ParentDivisionCode);
            int parentDivisionNameIndex = reader.GetOrdinal(Constants.ColumnNames.ParentDivisionName);

            while (reader.Read())
            {
                var division = new DivisionCF()
                {
                    DivisionId = reader.GetInt32(divisionIdndex),
                    DivisionName = reader.GetString(divisionNameIndex),
                    DivisionCode = reader.GetString(divisionCodeIndex)
                };
                if (!reader.IsDBNull(parentIdIndex))
                {
                    division.Parent = new DivisionCF()
                    {
                        DivisionId = reader.GetInt32(parentIdIndex),
                        DivisionName = reader.GetString(parentDivisionNameIndex),
                        DivisionCode = reader.GetString(parentDivisionCodeIndex)
                    };
                }
                cFDivisions.Add(division);
            }
        }

        public static List<Location> GetLocations()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                return GetLocations(connection);
            }
        }

        public static List<Location> GetLocations(SqlConnection connection)
        {
            var locations = new List<Location>();
            using (var command = new SqlCommand(Constants.ProcedureNames.Configuration.GetLocations, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadLocations(reader, locations);
                }
            }
            return locations;
        }

        private static void ReadLocations(SqlDataReader reader, List<Location> locations)
        {
            if (!reader.HasRows) return;
            int locationIdIndex = reader.GetOrdinal(Constants.ColumnNames.LocationId);
            int locationCodeIndex = reader.GetOrdinal(Constants.ColumnNames.LocationCode);
            int locationNameIndex = reader.GetOrdinal(Constants.ColumnNames.LocationName);
            int parentIdIndex = reader.GetOrdinal(Constants.ColumnNames.ParentId);
            int parentLocationCodeIndex = reader.GetOrdinal(Constants.ColumnNames.ParentLocationCode);
            int timeZoneIndex = reader.GetOrdinal(Constants.ColumnNames.TimeZone);
            int countryIndex = reader.GetOrdinal(Constants.ColumnNames.Country);

            while (reader.Read())
            {
                var location = new Location()
                {
                    LocationId = reader.GetInt32(locationIdIndex),
                    LocationCode = reader.GetString(locationCodeIndex),
                    LocationName = reader.GetString(locationNameIndex),
                    Country = reader.GetString(countryIndex),
                    TimeZone = reader.GetString(timeZoneIndex)
                };
                if (!reader.IsDBNull(parentIdIndex))
                {
                    location.Parent = new Location()
                    {
                        LocationId = reader.GetInt32(parentIdIndex),
                        LocationCode = reader.GetString(parentLocationCodeIndex)
                    };
                }
                locations.Add(location);
            }
        }
    }
}

