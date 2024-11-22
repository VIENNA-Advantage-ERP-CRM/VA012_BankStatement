using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    public class VA012_BankingJournalWidgetModel
    {
        //[NonSerialized]
        private Assembly asm = null;
        private Type type = null;
        private MethodInfo methodInfo = null;


        public int GetBankStatementFormID(Ctx ctx, string formName)
        {
            int FormID = Util.GetValueOfInt(DB.ExecuteScalar($"SELECT AD_Form_ID FROM AD_Form WHERE Name = {GlobalVariable.TO_STRING(formName)}"));
            return FormID;
        }

        public int CheckStatementExist(Ctx ctx, int C_BankAccount_ID)
        {
            int C_BankStatement_ID = Util.GetValueOfInt(DB.ExecuteScalar($@"SELECT C_BankStatement_ID FROM C_BankStatement WHERE IsActive='Y' 
                        AND DocStatus NOT IN('RE','VO','CO','CL') AND AD_Client_ID = { ctx.GetAD_Client_ID()} AND C_BANKACCOUNT_ID={C_BankAccount_ID}")); ;
            return C_BankStatement_ID;
        }

        /// <summary>
        /// Get Folder of login user.
        /// </summary>
        /// <param name="ctx">current context.</param> 
        /// <param name="userID">Logged in user.</param>
        /// <param name="isLoadOnDemand">Load on demand setting on dms settings window.</param>
        /// <param name="roleID">Role of the user.</param>
        /// <param name="clientID">Tenant of the user.</param>
        /// <param name="orgID">Organization of the user.</param>
        /// <returns>List of folder properties class type.</returns> 
        public object GetFolders(Ctx ctx, int userID, int roleID, int clientID, int orgID)
        {
            object result = "";
            #region DMS service using reflection
            string className = "VADMS.Services.DmsWebService";
            asm = System.Reflection.Assembly.Load("VADMSSvc");
            type = asm.GetType(className);
            if (type != null)
            {
                methodInfo = type.GetMethod("GetFolders");
                if (methodInfo != null)
                {
                    object classInstance = Activator.CreateInstance(type);
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 5)
                    {
                        object[] parametersArray = new object[] { ctx, userID, roleID, clientID, orgID };
                        result = methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }
            #endregion
            return result;
        }

        /// <summary>
        /// Check Folder access on folder
        /// </summary>
        /// <param name="folderID"></param>
        /// <returns></returns>
        public int CheckFolderAccess(int folderID, Ctx ctx)
        {
            object result = "";
            #region DMS service using reflection
            string className = "ViennaAdvantage.VADMS.Model.FolderModel";
            asm = System.Reflection.Assembly.Load("VADMSSvc");
            type = asm.GetType(className);
            if (type != null)
            {
                methodInfo = type.GetMethod("CheckFolderAccess");
                if (methodInfo != null)
                {
                    object classInstance = Activator.CreateInstance(type);
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 2)
                    {
                        object[] parametersArray = new object[] { folderID, ctx };
                        result = methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }
            #endregion
            return Util.GetValueOfInt(result);
        }
        /// <summary>
        /// Get Document from folder when user click on folder
        /// </summary>
        /// <param name="CurrrentPage">Current page of of the folder list</param>
        /// <param name="folderId">Folder id from which document has to be fetched</param>
        /// <param name="Window_ID">Current widow id</param>
        /// <param name="record_ID">Record id of the selected/opened window</param>
        /// <param name="isAdvancedSearch">Check if this is advance search, if not then create window query</param>
        /// <param name="isRecursive">Check if its recursive search, if yes then create document query</param>
        /// <param name="isSubscribeDoc">Check if current user has subscribed to this document then create subscription based document query</param>
        /// <param name="folType">Folder type ex. Inbox, draft or any other</param>
        /// <param name="ctx">Current context</param>
        /// <param name="pageSize">Number of rows to be fetch for current folder</param>
        /// <param name="AD_Table_ID">Optional table id, which is 0 by default</param>
        /// <returns>List of document properties class type</returns>
        public object LoadDocument(int CurrrentPage, string folderId, int Window_ID, int record_ID, bool isAdvancedSearch, bool isRecursive, bool isSubscribeDoc, string folType,
        Ctx ctx, int pageSize, int AD_Table_ID = 0, string orderBy = "")
        {
            object result = "";
            #region DMS service using reflection
            string className = "ViennaAdvantage.VADMS.Model.DocumentModel";
            asm = System.Reflection.Assembly.Load("VADMSSvc");
            type = asm.GetType(className);
            if (type != null)
            {
                methodInfo = type.GetMethod("LoadDocument");
                if (methodInfo != null)
                {
                    object classInstance = Activator.CreateInstance(type);
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 12)
                    {
                        object[] parametersArray = new object[] { CurrrentPage, folderId,  Window_ID,  record_ID,  isAdvancedSearch,  isRecursive,  isSubscribeDoc,  folType,
                        ctx,  pageSize,  AD_Table_ID ,  orderBy};
                        result = methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }
            #endregion
            return result;
        }
        /// <summary>
        /// Download document in Tempdownload folder based on document id
        /// </summary>
        /// <param name="dmsToken">DMS Token</param>
        /// <param name="documentID">Document id which need to be Download</param>
        /// <returns>String, link of document for download</returns>
        public Dictionary<string, string> GetMetaData(int documentID, Ctx ctx)
        {
            dynamic result = "";
            dynamic filePath = "";
            string downloadlink = "";
            Dictionary<string, string> data = new Dictionary<string, string>();
            #region DMS service using reflection
            string className = "ViennaAdvantage.VADMS.Model.DocumentModel";
            asm = System.Reflection.Assembly.Load("VADMSSvc");
            type = asm.GetType(className);
            if (type != null)
            {
                //Get Meta data
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "GetMetaData").ToArray();
                Type[] parameterTypes = new Type[] { typeof(int), typeof(bool), typeof(int), typeof(Ctx), typeof(int), typeof(int), typeof(int) };
                MethodInfo methodInfo = methods.FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
                if (methodInfo != null)
                {
                    object classInstance = Activator.CreateInstance(type);
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 7)
                    {
                        object[] parametersArray = new object[] { documentID, false, 0, ctx, 1, 1000, documentID };
                        result = methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }
            #endregion
            if (result != null)
            {
                className = "";
                asm = null;
                #region DMS service using reflection
                className = "ViennaAdvantage.VADMS.Model.DocumentModel";
                asm = System.Reflection.Assembly.Load("VADMSSvc");
                type = asm.GetType(className);
                //Get file path
                MethodInfo[] method = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "GetAttachmentLineFilePath").ToArray();
                Type[] parameterType = new Type[] { typeof(Ctx), typeof(int) };
                MethodInfo methodInfo = method.FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterType));
                if (methodInfo != null)
                {
                    object classInstance = Activator.CreateInstance(type);
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length == 2)
                    {
                        object[] parametersArray = new object[] { ctx, result[0].LstMetaData[0].AttachmentID };
                        filePath = methodInfo.Invoke(classInstance, parametersArray);
                        if (!Directory.Exists(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload")))
                        {
                            Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload"));
                        }
                        // Download file in TempDownload folder
                        downloadlink = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload", filePath);
                    }
                }
                #endregion
            }
            //return filename and file path
            data.Add("_filename", result[0].LstMetaData[0].Document + "" + result[0].LstMetaData[0].FileType);
            data.Add("_path", downloadlink);
            return data;
        }
    }
}