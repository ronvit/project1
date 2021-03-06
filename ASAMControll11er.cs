﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Ch.Elca.Iiop;
using Ch.Elca.Iiop.Services;
using omg.org.CosNaming;
using org.asam.ods;
using System.Web;
using System.Web.Mvc;
using FinalReports.Models;
namespace FinalReports.Controllers
{
    public class ASAMController : Controller
    {
        static ValueMatrix vm;
        public string GetUnit(int testId, string chname, AoSession aoSession)
        {
            string unit = "";

            String aeName = "Channel";
            String[] aaNames = new String[] { "Name_Of_The_Channel" };
            ApplicationStructure asobj = aoSession.getApplicationStructure();
            ApplElemAccess aea = aoSession.getApplElemAccess();
            ApplicationElement aeRun = asobj.getElementsByBaseType("AoMeasurement")[0];
            ApplicationElement ae = asobj.getElementByName(aeName);
            ApplicationElement aeUnit = asobj.getElementsByBaseType("AoUnit")[0];
            T_LONGLONG aidRun = aeRun.getId();
            T_LONGLONG aid = ae.getId();
            QueryStructureExt aoq = new QueryStructureExt();
            aoq.anuSeq = new SelAIDNameUnitId[aaNames.Length + 1];
            for (int i = 0; i < aaNames.Length; i++)
            {
                aoq.anuSeq[i] = createAnu(aid, aaNames[i]);
            }
            aoq.anuSeq[aaNames.Length] = createAnu(aeUnit.getId(), "Name");
            ApplicationAttribute aaRunName = aeRun.getAttributeByBaseName("name");
            aoq.condSeq = new SelItem[1];
            aoq.condSeq[0] = new SelItem();
            SelValueExt selValue = new SelValueExt();
            selValue.attr = new AIDNameUnitId(new AIDName(aidRun, "TestRequestId"), new T_LONGLONG(0, 0));
            selValue.oper = SelOpcode.EQ;
            selValue.value = new TS_Value(new TS_Union(), (short)15);

            selValue.value.u.SetlongVal(testId);
            aoq.condSeq[0].Setvalue(selValue);
            aoq.orderBy = new SelOrder[0];
            aoq.groupBy = new AIDName[0];
            aoq.joinSeq = new JoinDef[2]; // Use the default join created by ASAM ODS server.
            aoq.joinSeq[0].fromAID = ae.getId();
            aoq.joinSeq[0].toAID = aeUnit.getId();
            aoq.joinSeq[0].joiningType = JoinType.JTOUTER;
            aoq.joinSeq[0].refName = "unit";

            aoq.joinSeq[1].fromAID = ae.getId();
            aoq.joinSeq[1].toAID = aeRun.getId();
            aoq.joinSeq[1].joiningType = JoinType.JTDEFAULT;
            aoq.joinSeq[1].refName = "safety_Test";

            ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
            string[] channels = res[0].firstElems[0].values[0].value.u.GetstringVal();
            string[] units = res[0].firstElems[1].values[0].value.u.GetstringVal();
            int index = 0;
            foreach (string channel in channels)
            {
                if (channel.Equals(chname))
                {
                    unit = units[index];
                    break;
                }
                index++;
            }
            return unit;
        }
        public AoSession Con_NameService()
        {
            AoSession session = null;
            try
            {
                // Initialize a CORBA client channel.
                //IiopClientChannel channel1 = new IiopClientChannel();


                foreach (IChannel objChannel in ChannelServices.RegisteredChannels)
                    ChannelServices.UnregisterChannel(objChannel);
                IiopClientChannel channel1 = new IiopClientChannel();
                ChannelServices.RegisterChannel(channel1, false);

                CorbaInit init = CorbaInit.GetInit();


                NamingContext nameService = init.GetNameService(ConfigurationManager.AppSettings["ASAMServer"], Convert.ToInt32(ConfigurationManager.AppSettings["ASAMPort"]));


                NameComponent[] name = new NameComponent[] { new NameComponent(ConfigurationManager.AppSettings["ASAMDB"], "ASAM-ODS") };


                Object obj = nameService.resolve(name);
                AoFactory aoFactory = (AoFactory)obj;


                // if (ConfigurationManager.AppSettings["ASAMServer"].ToLower() == "hraovcatavalond")
                //{

                    //string userName = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).Identity.Name;
                    //if (String.IsNullOrEmpty(userName))
                    //{
                    //    userName = Request.ServerVariables["REMOTE_USER"].ToString();
                    //}
                    string userName = Request.ServerVariables["REMOTE_USER"].ToString();
                    if (String.IsNullOrEmpty(userName))
                    {
                        userName = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).Identity.Name;
                    }
                string userid = userName.Substring(userName.IndexOf(@"\") + 1);

                session = aoFactory.newSession(System.Configuration.ConfigurationManager.AppSettings["ASAMconnection"] + ",FOR_USER=" + userid);
                //}
                // else
                // {
                //     session = aoFactory.newSession(System.Configuration.ConfigurationManager.AppSettings["ASAMconnection"]);
                // }
                //ApplicationStructure asObj = session.getApplicationStructure();

                // Response.Write("Connection was successfully established.");

            }
            catch (Exception aoe) //AoException aoe
            {

                // Response.Write(aoe.Message);//aoe.reason
                //throw aoe;
            }
            // if(session!=null)
            return session;

        }
        public ValueMatrix GetValueMatrix(int testId, AoSession aoSession)
        {
            ApplicationStructure asObj = null;


            String aeName = "Submatrix";

            String[] aaNames = new String[] { "*" };

            // aoSession = Con_NameService();
            asObj = aoSession.getApplicationStructure();
            //ListTests();
            ApplElemAccess aea = aoSession.getApplElemAccess();
            ApplicationElement ae = asObj.getElementByName(aeName);
            ApplicationElement aeMea = asObj.getElementsByBaseType("AoMeasurement")[0];

            T_LONGLONG aidMea = aeMea.getId();
            T_LONGLONG aid = ae.getId();
            QueryStructureExt aoq = new QueryStructureExt();
            aoq.anuSeq = new SelAIDNameUnitId[aaNames.Length];
            for (int i = 0; i < aaNames.Length; i++)
            {
                aoq.anuSeq[i] = createAnu(aid, aaNames[i]);
            }
            aoq.condSeq = new SelItem[1];
            aoq.condSeq[0] = new SelItem();
            SelValueExt selValue = new SelValueExt();
            selValue.attr = new AIDNameUnitId(new AIDName(aidMea, "TestRequestId"), new T_LONGLONG(0, 0));
            selValue.oper = SelOpcode.EQ;
            selValue.value = new TS_Value(new TS_Union(), (short)15);
            selValue.value.u.SetlongVal(testId);
            aoq.condSeq[0].Setvalue(selValue);
            aoq.orderBy = new SelOrder[0];
            aoq.groupBy = new AIDName[0];
            aoq.joinSeq = new JoinDef[0];
            ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
            T_LONGLONG aidSm = ae.getId();
            string[] datFileNames = res[0].firstElems[0].values[0].value.u.GetstringVal();
            T_LONGLONG[] ieIds = res[0].firstElems[0].values[1].value.u.GetlonglongVal();
            int j = 0;
            for (; j < datFileNames.Length; j++)
            {
                if (!datFileNames[j].Contains("LCB_DIAdemHeader"))
                {
                    break;
                }
            }

            vm = aea.getValueMatrix(new ElemId(aidSm, ieIds[j]));

            return vm;
        }

        public string GetRequestName(int testId, AoSession aoSession)
        {
            string result = null;
            try
            {

                ApplicationStructure asObj = aoSession.getApplicationStructure();

                String aeName = "Safety_Test";

                String[] aaNames = new String[] { "Name" };

                //aoSession = Con_NameService();
                // asObj = aoSession.getApplicationStructure();
                ApplElemAccess aea = aoSession.getApplElemAccess();
                ApplicationElement ae = asObj.getElementByName(aeName);
                ApplicationElement aeMea = asObj.getElementsByBaseType("AoMeasurement")[0];

                T_LONGLONG aidMea = aeMea.getId();
                T_LONGLONG aid = ae.getId();
                QueryStructureExt aoq = new QueryStructureExt();
                aoq.anuSeq = new SelAIDNameUnitId[aaNames.Length];
                for (int i = 0; i < aaNames.Length; i++)
                {
                    aoq.anuSeq[i] = createAnu(aid, aaNames[i]);
                }
                aoq.condSeq = new SelItem[1];
                aoq.condSeq[0] = new SelItem();
                SelValueExt selValue = new SelValueExt();
                selValue.attr = new AIDNameUnitId(new AIDName(aidMea, "TestRequestId"), new T_LONGLONG(0, 0));
                selValue.oper = SelOpcode.EQ;
                selValue.value = new TS_Value(new TS_Union(), (short)15);
                selValue.value.u.SetstringVal(testId.ToString());
                aoq.condSeq[0].Setvalue(selValue);
                aoq.orderBy = new SelOrder[0];
                aoq.groupBy = new AIDName[0];
                aoq.joinSeq = new JoinDef[0];
                ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
                result = showTableResults(res[0]);

            }
            catch (AoException ex)
            {
            }

            return result;
        }


        public string GetImpactSpeed(int testId, AoSession aoSession)
        {
            string result = null;
            try
            {

                ApplicationStructure asObj = aoSession.getApplicationStructure();

                String aeName = "Safety_Test";

                String[] aaNames = new String[] { "ImpactSpeed" };

                //aoSession = Con_NameService();
                // asObj = aoSession.getApplicationStructure();
                ApplElemAccess aea = aoSession.getApplElemAccess();
                ApplicationElement ae = asObj.getElementByName(aeName);
                ApplicationElement aeMea = asObj.getElementsByBaseType("AoMeasurement")[0];

                T_LONGLONG aidMea = aeMea.getId();
                T_LONGLONG aid = ae.getId();
                QueryStructureExt aoq = new QueryStructureExt();
                aoq.anuSeq = new SelAIDNameUnitId[aaNames.Length];
                for (int i = 0; i < aaNames.Length; i++)
                {
                    aoq.anuSeq[i] = createAnu(aid, aaNames[i]);
                }
                aoq.condSeq = new SelItem[1];
                aoq.condSeq[0] = new SelItem();
                SelValueExt selValue = new SelValueExt();
                selValue.attr = new AIDNameUnitId(new AIDName(aidMea, "TestRequestId"), new T_LONGLONG(0, 0));
                selValue.oper = SelOpcode.EQ;
                selValue.value = new TS_Value(new TS_Union(), (short)15);
                selValue.value.u.SetstringVal(testId.ToString());
                aoq.condSeq[0].Setvalue(selValue);
                aoq.orderBy = new SelOrder[0];
                aoq.groupBy = new AIDName[0];
                aoq.joinSeq = new JoinDef[0];
                ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
                result = showTableResults(res[0]);

            }
            catch (AoException ex)
            {
            }

            return result;
        }

        public String[] GetImpactSide(int testId, AoSession aoSession)
        {
            String[] result = new String[2];
            try
            {

                ApplicationStructure asObj = aoSession.getApplicationStructure();

                String aeName = "Safety_Test";

                String[] aaNames = new String[] { "ImpactSide", "ImpactBarrier" };

                //aoSession = Con_NameService();
                asObj = aoSession.getApplicationStructure();
                ApplElemAccess aea = aoSession.getApplElemAccess();
                ApplicationElement ae = asObj.getElementByName(aeName);
                ApplicationElement aeMea = asObj.getElementsByBaseType("AoMeasurement")[0];

                T_LONGLONG aidMea = aeMea.getId();
                T_LONGLONG aid = ae.getId();
                QueryStructureExt aoq = new QueryStructureExt();
                aoq.anuSeq = new SelAIDNameUnitId[aaNames.Length];
                for (int i = 0; i < aaNames.Length; i++)
                {
                    aoq.anuSeq[i] = createAnu(aid, aaNames[i]);
                }
                aoq.condSeq = new SelItem[1];
                aoq.condSeq[0] = new SelItem();
                SelValueExt selValue = new SelValueExt();
                selValue.attr = new AIDNameUnitId(new AIDName(aidMea, "TestRequestId"), new T_LONGLONG(0, 0));
                selValue.oper = SelOpcode.EQ;
                selValue.value = new TS_Value(new TS_Union(), (short)15);
                selValue.value.u.SetlongVal(testId);
                aoq.condSeq[0].Setvalue(selValue);
                aoq.orderBy = new SelOrder[0];
                aoq.groupBy = new AIDName[0];
                aoq.joinSeq = new JoinDef[0];
                ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
                //  result = showTableResults(res[0]);
                ArrayList jj = showTableResult_Calc(res, aid, "ImpactSide");
                ArrayList ss = showTableResult_Calc(res, aid, "ImpactBarrier");
                String[] myArr1 = (String[])jj.ToArray(typeof(string));
                String[] myArr2 = (String[])ss.ToArray(typeof(string));
                result[0] = myArr1[0];
                result[1] = myArr2[0];
            }
            catch (AoException ex)
            {
            }

            return result;
        }
        //get general info of a test
        private static ArrayList showTableResult_Calc(ResultSetExt[] res, T_LONGLONG aid, string columnName)
        {
            ArrayList kk = new ArrayList();
            String attrValue = null;
            String[] entityNames = null;
            String[] attributeNames = null;
            String[][] aliasNames = null;
            if ((res != null) && (res.Length == 1))
            {
                //if ((res[0].firstElems != null) && (res[0].firstElems.Length == 1))
                if ((res[0].firstElems != null))
                {
                    for (int i = 0; i < res[0].firstElems.Length; i++)
                    {
                        //if (res[0].firstElems[i].aid.low == aid.low)
                        // {
                        if ((res[0].firstElems[i].values != null) && (res[0].firstElems[i].values.Length > 0))
                        {
                            for (int j = 0; j < res[0].firstElems[i].values.Length; j++)
                            {
                                String attrName = res[0].firstElems[i].values[j].valName;
                                if (columnName.ToUpper() == attrName.ToUpper())
                                {
                                    // String attrValue = null;
                                    //if (res[0].firstElems[i].values[j].value.flag[0] == 15)
                                    //{ // Only one attribute value so only one flag
                                    switch (res[0].firstElems[i].values[j].value.u.Discriminator)
                                    {
                                        case DataType.DT_STRING:
                                            {
                                                String[] val = res[0].firstElems[i].values[j].value.u.GetstringVal();
                                                if (val.Count() > 0)
                                                {
                                                    attrValue = val[0];
                                                    kk.AddRange(val);
                                                }
                                                break;
                                            }
                                        case DataType.DT_FLOAT:
                                            {
                                                float[] val = res[0].firstElems[i].values[j].value.u.GetfloatVal();
                                                attrValue = "" + val[0];
                                                kk.AddRange(val);
                                                break;
                                            }
                                        case DataType.DT_DOUBLE:
                                            {
                                                double[] val = res[0].firstElems[i].values[j].value.u.GetdoubleVal();
                                                attrValue = "" + val[0];
                                                kk.AddRange(val);
                                                break;
                                            }
                                        case DataType.DT_LONG:
                                            {
                                                int[] val = res[0].firstElems[i].values[j].value.u.GetlongVal();
                                                attrValue = "" + val[0];
                                                kk.AddRange(val);
                                                break;
                                            }

                                        case DataType.DT_LONGLONG:
                                            {
                                                T_LONGLONG[] val = res[0].firstElems[i].values[j].value.u.GetlonglongVal();
                                                int[] list1 = new int[val.Length];
                                                //vals[colIndex] = Array.ConvertAll<T_LONGLONG, string>(tll, Convert.ToString);
                                                for (int l = 0; l < val.Length; l++)
                                                    list1[i] = val[l].high;

                                                //  attrValue = "" + val[0];
                                                kk.AddRange(list1);
                                                break;
                                            }
                                        case DataType.DT_DATE:
                                            {
                                                string formatDate = "yyyyMMdHHmmssffffff";
                                                String[] val = res[0].firstElems[i].values[j].value.u.GetdateVal();

                                                DateTime[] list1 = new DateTime[val.Length];
                                                //vals[colIndex] = Array.ConvertAll<T_LONGLONG, string>(tll, Convert.ToString);
                                                for (int l = 0; l < val.Length; l++)
                                                    list1[l] = DateTime.ParseExact(val[l], formatDate.Substring(0, val[l].ToString().Length - 1),
                                                     System.Globalization.CultureInfo.InvariantCulture);

                                                kk.AddRange(list1);

                                                //vals[colIndex] = elemRes.firstElems[colIndex].values[0].value.u.GetdateVal();
                                                //DateTime myDate = DateTime.ParseExact(vals[colIndex][0].ToString(), formatDate.Substring(0, vals[colIndex][0].ToString().Length - 1),
                                                //      System.Globalization.CultureInfo.InvariantCulture);
                                                //vals[colIndex] = myDate.ToString().Split(',');
                                                break;
                                            }
                                        default:
                                            attrValue = "Not implemented datatype " + res[0].firstElems[i].values[j].value.u.Discriminator.ToString();
                                            break;
                                    }
                                }
                                //}
                                //else
                                //{
                                //    attrValue = "Undefined";
                                //}
                                //  Console.WriteLine(attrName + "=" + attrValue);
                            }
                        }

                        // }
                    }
                }
            }

            return kk;
        }
        public static string showTableResults(ResultSetExt elemRes)
        {
            string result = null;
            int noRows;
            int noCols;
            if (elemRes.firstElems != null)
            {
                noCols = elemRes.firstElems.Length;
                //    noCols = 2;
                String[][] vals = new String[noCols + 1][];
                short[][] flags = new short[noCols][];
                // int[][] ss = new int[noCols][];
                for (int colIndex = 0; colIndex < noCols; colIndex++)
                {
                    DataType dt = elemRes.firstElems[colIndex].values[0].value.u.Discriminator;
                    switch (dt)
                    {
                        case DataType.DT_STRING:
                            {
                                vals[colIndex] = elemRes.firstElems[colIndex].values[0].value.u.GetstringVal();
                                break;
                            }
                        case DataType.DT_DATE:
                            {
                                string formatDate = "yyyyMMdHHmmssffffff";
                                vals[colIndex] = elemRes.firstElems[colIndex].values[0].value.u.GetdateVal();
                                DateTime myDate = DateTime.ParseExact(vals[colIndex][0].ToString(), formatDate.Substring(0, vals[colIndex][0].ToString().Length - 1),
                                      System.Globalization.CultureInfo.InvariantCulture);
                                vals[colIndex] = myDate.ToString().Split(',');
                                break;
                            }
                        case DataType.DT_DOUBLE:
                            {
                                double[] tempDouble;

                                tempDouble = elemRes.firstElems[colIndex].values[0].value.u.GetdoubleVal();
                                vals[colIndex] = Array.ConvertAll<double, string>(tempDouble, Convert.ToString);
                                break;
                            }
                        case DataType.DT_FLOAT:
                            {
                                float[] tempDouble;

                                tempDouble = elemRes.firstElems[colIndex].values[0].value.u.GetfloatVal();
                                vals[colIndex] = Array.ConvertAll<float, string>(tempDouble, Convert.ToString);
                                break;
                            }
                        case DataType.DT_LONG:
                            {
                                int[] tempDouble;

                                tempDouble = elemRes.firstElems[0].values[0].value.u.GetlongVal();
                                vals[colIndex] = Array.ConvertAll<int, string>(tempDouble, Convert.ToString);
                                break;
                            }
                    }

                    flags[colIndex] = elemRes.firstElems[colIndex].values[0].value.flag;
                    // ss[colIndex ] = elemRes.firstElems[colIndex].values[1].value.u.GetlongVal();
                }
                if (noCols > 0)
                {
                    noRows = vals[0].Length;
                    // Console.WriteLine("Number of rows: " + noRows);
                    for (int rowIndex = 0; rowIndex < noRows; rowIndex++)
                    {
                        for (int colIndex = 0; colIndex < noCols; colIndex++)
                        {
                            if (flags[colIndex][rowIndex] == 15)
                            {
                                result = vals[colIndex][rowIndex].ToString();
                            }

                        }

                    }
                }
            }
            return result;
        }

        public double[] getData(ValueMatrix vm, String colName, int startIndex, int endIndex)
        {
            double[] tempDouble = null, resultDouble = null;
            try
            {
                Column[] cols = vm.getColumns(colName);
                TS_ValueSeq vals = vm.getValueVector(cols[0], startIndex, endIndex - startIndex);
                DataType dt = vals.u.Discriminator;
                switch (dt)
                {

                    case DataType.DT_DOUBLE:
                        {

                            tempDouble = vals.u.GetdoubleVal();
                            break;
                        }
                }
                cols[0].destroy();
                //if (tempDouble != null && endIndex != 0)
                //{
                //  resultDouble = new double[endIndex - startIndex + 1];
                //  Array.Copy(tempDouble, startIndex, resultDouble, 0, endIndex - startIndex + 1);
                //}
                //else
                //{
                resultDouble = tempDouble;
                //}
            }
            catch (Exception ex)
            {

            }
            return resultDouble;

        }
        public double[] getData(ValueMatrix vm, String colName)
        {
            double[] tempDouble = null;
            try
            {
                Column[] cols = vm.getColumns(colName);
                TS_ValueSeq vals = vm.getValueVector(cols[0], 0, 0);
                DataType dt = vals.u.Discriminator;
                switch (dt)
                {

                    case DataType.DT_DOUBLE:
                        {

                            tempDouble = vals.u.GetdoubleVal();
                            break;
                        }
                }
                cols[0].destroy();
            }
            catch (Exception ex)
            {

            }
            return tempDouble;

        }

        public double[] getColumnData(ValueMatrix vm, Column colName)
        {
            double[] tempDouble = null;
            try
            {

                TS_ValueSeq vals = vm.getValueVector(colName, 0, 0);
                DataType dt = vals.u.Discriminator;
                switch (dt)
                {

                    case DataType.DT_DOUBLE:
                        {

                            tempDouble = vals.u.GetdoubleVal();
                            break;
                        }
                }
                //   cols[0].destroy();
            }
            catch (Exception ex)
            {

            }
            return tempDouble;

        }





        public string GetATDType(int testId, string p, AoSession aoSession)
        {
            string result = null;
            ArrayList Testres = null;
            try
            {

                String aeName = "ATD";
                String[] aaNames = new String[] { "SelectedAtdType", "Version" };
                String attrValue = null;

                ApplicationStructure asobj = aoSession.getApplicationStructure();
                ApplElemAccess aea = aoSession.getApplElemAccess();
                ApplicationElement aeRun = asobj.getElementsByBaseType("AoMeasurement")[0];
                ApplicationElement ae = asobj.getElementByName(aeName);

                T_LONGLONG aidRun = aeRun.getId();
                T_LONGLONG aid = ae.getId();
                QueryStructureExt aoq = new QueryStructureExt();
                aoq.anuSeq = new SelAIDNameUnitId[aaNames.Length];
                for (int i = 0; i < aaNames.Length; i++)
                {
                    aoq.anuSeq[i] = createAnu(aid, aaNames[i]);
                }
                //  aoq.anuSeq[aaNames.Length] = createAnu(aidRun, "Name");
                ApplicationAttribute aaRunName = aeRun.getAttributeByName("TestRequestId");
                aoq.condSeq = new SelItem[3];
                aoq.condSeq[0] = new SelItem();
                SelValueExt selValue = new SelValueExt();
                selValue.attr = new AIDNameUnitId(new AIDName(aidRun, aaRunName.getName()), new T_LONGLONG(0, 0));
                selValue.oper = SelOpcode.EQ;
                selValue.value = new TS_Value(new TS_Union(), (short)15);
                //selValue.value.u.SetstringVal("EPA75");
                selValue.value.u.SetstringVal(testId.ToString());
                aoq.condSeq[0].Setvalue(selValue);


                // AND
                aoq.condSeq[1] = new SelItem();
                aoq.condSeq[1].Setoperator(SelOperator.AND);


                //ApplicationAttribute secName = ae.getAttributeByName("Version");
                aoq.condSeq[2] = new SelItem();
                SelValueExt selValue1 = new SelValueExt();
                selValue1.attr = new AIDNameUnitId(new AIDName(aid, "Version"), new T_LONGLONG(0, 0));
                selValue1.oper = SelOpcode.CI_LIKE;
                selValue1.value = new TS_Value(new TS_Union(), (short)15);
                //selValue.value.u.SetstringVal("EPA75");
                selValue1.value.u.SetstringVal(p + "*");
                aoq.condSeq[2].Setvalue(selValue1);

                aoq.orderBy = new SelOrder[0];
                aoq.groupBy = new AIDName[0];
                aoq.joinSeq = new JoinDef[0];

                ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);

                Testres = showTableResult_Calc(res, aid, "SelectedAtdType");
                if (Testres.Count > 0)
                    result = Testres[0].ToString();
            }
            catch (AoException ex)
            {
            }

            return result;
        }

        public List<string> GetProjectList(string testTypeName, AoSession aoSession)
        {
            if (aoSession == null)
            {
                aoSession = Con_NameService();
            }
            List<string> defaultList = new List<string>();
            //try
            //{
            ApplicationStructure asobj = aoSession.getApplicationStructure();
            ApplElemAccess aea = aoSession.getApplElemAccess();
            ApplicationElement aeProject = asobj.getElementByName("Project");
            ApplicationElement aeTestType = asobj.getElementByName("Type_Of_Test");

            //Get element Ids
            T_LONGLONG aIdProject = aeProject.getId();
            T_LONGLONG aIdTestType = aeTestType.getId();
            //Construct query
            QueryStructureExt aoq = new QueryStructureExt();
            //Define attributes needed to return
            aoq.anuSeq = new SelAIDNameUnitId[1];
            aoq.anuSeq[0] = createAnu(aIdProject, "Name");
            //aoq.anuSeq[1] = createAnu(aIdTest, "TestRequestId");
            //Define conditions for the query
            // first condition
            aoq.condSeq = new SelItem[1];
            aoq.condSeq[0] = new SelItem();

            SelValueExt selValue = new SelValueExt();
            // left part of the condition
            ApplicationAttribute aaRunName = aeTestType.getAttributeByBaseName("Name");
            selValue.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaRunName.getName()), new T_LONGLONG(0, 0));
            // operator
            selValue.oper = SelOpcode.LIKE;
            //right part
            selValue.value = new TS_Value(new TS_Union(), (short)15);
            selValue.value.u.SetstringVal(testTypeName);
            aoq.condSeq[0].Setvalue(selValue);
            // initilize other parts
            aoq.orderBy = new SelOrder[0];
            aoq.groupBy = new AIDName[0];
            aoq.joinSeq = new JoinDef[0];


            // execute the query

            ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
            ArrayList projectName = showTableResult_Calc(res, aIdProject, "Name");


            defaultList = projectName.ToArray().Select(x => x.ToString()).Distinct().ToList();

            //}
            //catch (Exception)
            //{


            //}
            //finally
            //{
            //  if (aoSession != null)
            //    aoSession.close();
            //}
            return defaultList;
        }

        public List<string> GetFrameCodeList(string[] projectName, string testtype, AoSession aoSession)
        {
            if (aoSession == null)
            {
                aoSession = Con_NameService();
            }
            List<string> defaultList = new List<string>();
            //try
            //{
            ApplicationStructure asobj = aoSession.getApplicationStructure();
            ApplElemAccess aea = aoSession.getApplElemAccess();
            ApplicationElement aeFrameCode = asobj.getElementByName("FrameCode");
            ApplicationElement aeProject = asobj.getElementByName("Project");
            ApplicationElement aeTestType = asobj.getElementByName("Type_Of_Test");

            //Get element Ids
            T_LONGLONG aIdFrameCode = aeFrameCode.getId();
            T_LONGLONG aIdProject = aeProject.getId();
            T_LONGLONG aIdTestType = aeTestType.getId();
            //Construct query
            QueryStructureExt aoq = new QueryStructureExt();
            //Define attributes needed to return
            aoq.anuSeq = new SelAIDNameUnitId[1];
            aoq.anuSeq[0] = createAnu(aIdFrameCode, "FrameCode");
            //aoq.anuSeq[1] = createAnu(aIdTest, "TestRequestId");
            //Define conditions for the query
            // first condition
            if (projectName.Length == 1)
            {
                aoq.condSeq = new SelItem[projectName.Length * 2 + 1];
                aoq.condSeq[0] = new SelItem();
                SelValueExt selValue = new SelValueExt();
                // left part of the condition
                ApplicationAttribute aaRunName = aeProject.getAttributeByBaseName("Name");
                selValue.attr = new AIDNameUnitId(new AIDName(aIdProject, aaRunName.getName()), new T_LONGLONG(0, 0));
                // operator
                selValue.oper = SelOpcode.LIKE;
                //right part
                selValue.value = new TS_Value(new TS_Union(), (short)15);
                selValue.value.u.SetstringVal(projectName[0]);
                aoq.condSeq[0].Setvalue(selValue);


                aoq.condSeq[1] = new SelItem();
                aoq.condSeq[1].Setoperator(SelOperator.AND);

                // third condition
                aoq.condSeq[2] = new SelItem();
                SelValueExt selValue2 = new SelValueExt();
                // left part of the condition
                ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                // operator
                selValue2.oper = SelOpcode.LIKE;
                //right part
                selValue2.value = new TS_Value(new TS_Union(), (short)15);
                selValue2.value.u.SetstringVal(testtype);
                aoq.condSeq[2].Setvalue(selValue2);

            }
            else
            {
                aoq.condSeq = new SelItem[projectName.Length * 2 + 3];
                int count = 0;
                aoq.condSeq[0] = new SelItem();
                aoq.condSeq[0].Setoperator(SelOperator.OPEN);
                for (int i = 1; i < projectName.Length + 1; i++)
                {
                    aoq.condSeq[i + count] = new SelItem();
                    SelValueExt selValue = new SelValueExt();
                    // left part of the condition
                    ApplicationAttribute aaRunName = aeProject.getAttributeByBaseName("Name");
                    selValue.attr = new AIDNameUnitId(new AIDName(aIdProject, aaRunName.getName()), new T_LONGLONG(0, 0));
                    // operator
                    selValue.oper = SelOpcode.LIKE;
                    //right part
                    selValue.value = new TS_Value(new TS_Union(), (short)15);
                    selValue.value.u.SetstringVal(projectName[i - 1]);
                    aoq.condSeq[i + count].Setvalue(selValue);

                    // second condition
                    if (i != projectName.Length)
                    {
                        aoq.condSeq[i + 1 + count] = new SelItem();
                        aoq.condSeq[i + 1 + count].Setoperator(SelOperator.OR);

                    }
                    count = i;
                }

                aoq.condSeq[count * 2] = new SelItem();
                aoq.condSeq[count * 2].Setoperator(SelOperator.CLOSE);

                aoq.condSeq[count * 2 + 1] = new SelItem();
                aoq.condSeq[count * 2 + 1].Setoperator(SelOperator.AND);

                // third condition
                aoq.condSeq[count * 2 + 2] = new SelItem();
                SelValueExt selValue2 = new SelValueExt();
                // left part of the condition
                ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                // operator
                selValue2.oper = SelOpcode.LIKE;
                //right part
                selValue2.value = new TS_Value(new TS_Union(), (short)15);
                selValue2.value.u.SetstringVal(testtype);
                aoq.condSeq[count * 2 + 2].Setvalue(selValue2);
            }
            // initilize other parts
            aoq.orderBy = new SelOrder[0];
            aoq.groupBy = new AIDName[0];
            aoq.joinSeq = new JoinDef[0];

            // execute the query

            ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
            ArrayList frameCodeList = showTableResult_Calc(res, aIdFrameCode, "FrameCode");


            defaultList = frameCodeList.ToArray().Select(x => x.ToString()).Distinct().ToList();
            return defaultList;
        }

        public List<string> GetTestModeList(string[] frameCodeName, string testtype, AoSession aoSession)
        {
            if (aoSession == null)
            {
                aoSession = Con_NameService();
            }
            List<string> defaultList = new List<string>();
            //try
            //{
            ApplicationStructure asobj = aoSession.getApplicationStructure();
            ApplElemAccess aea = aoSession.getApplElemAccess();
            ApplicationElement aeTestMode = asobj.getElementByName("Subtype_Of_Test");
            ApplicationElement aeFrameCode = asobj.getElementByName("FrameCode");
            ApplicationElement aeTestType = asobj.getElementByName("Type_Of_Test");

            //Get element Ids
            T_LONGLONG aIdTestMode = aeTestMode.getId();
            T_LONGLONG aIdFrameCode = aeFrameCode.getId();
            T_LONGLONG aIdTestType = aeTestType.getId();
            //Construct query
            QueryStructureExt aoq = new QueryStructureExt();
            //Define attributes needed to return
            aoq.anuSeq = new SelAIDNameUnitId[1];
            aoq.anuSeq[0] = createAnu(aIdTestMode, "Name");

            if (frameCodeName.Length == 1)
            {
                aoq.condSeq = new SelItem[frameCodeName.Length * 2 + 1];
                aoq.condSeq[0] = new SelItem();
                SelValueExt selValue = new SelValueExt();
                // left part of the condition
                ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
                selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
                // operator
                selValue.oper = SelOpcode.LIKE;
                //right part
                selValue.value = new TS_Value(new TS_Union(), (short)15);
                selValue.value.u.SetstringVal(frameCodeName[0]);
                aoq.condSeq[0].Setvalue(selValue);


                aoq.condSeq[1] = new SelItem();
                aoq.condSeq[1].Setoperator(SelOperator.AND);

                // third condition
                aoq.condSeq[2] = new SelItem();
                SelValueExt selValue2 = new SelValueExt();
                // left part of the condition
                ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                // operator
                selValue2.oper = SelOpcode.LIKE;
                //right part
                selValue2.value = new TS_Value(new TS_Union(), (short)15);
                selValue2.value.u.SetstringVal(testtype);
                aoq.condSeq[2].Setvalue(selValue2);

            }

            else
            {
                aoq.condSeq = new SelItem[frameCodeName.Length * 2 + 3];
                int count = 0;
                aoq.condSeq[0] = new SelItem();
                aoq.condSeq[0].Setoperator(SelOperator.OPEN);
                for (int i = 1; i < frameCodeName.Length + 1; i++)
                {
                    aoq.condSeq[i + count] = new SelItem();
                    SelValueExt selValue = new SelValueExt();
                    // left part of the condition
                    ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
                    selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
                    // operator
                    selValue.oper = SelOpcode.LIKE;
                    //right part
                    selValue.value = new TS_Value(new TS_Union(), (short)15);
                    selValue.value.u.SetstringVal(frameCodeName[i - 1]);
                    aoq.condSeq[i + count].Setvalue(selValue);

                    // second condition
                    if (i != frameCodeName.Length)
                    {
                        aoq.condSeq[i + 1 + count] = new SelItem();
                        aoq.condSeq[i + 1 + count].Setoperator(SelOperator.OR);

                    }
                    count = i;
                }

                aoq.condSeq[count * 2] = new SelItem();
                aoq.condSeq[count * 2].Setoperator(SelOperator.CLOSE);

                aoq.condSeq[count * 2 + 1] = new SelItem();
                aoq.condSeq[count * 2 + 1].Setoperator(SelOperator.AND);

                // third condition
                aoq.condSeq[count * 2 + 2] = new SelItem();
                SelValueExt selValue2 = new SelValueExt();
                // left part of the condition
                ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                // operator
                selValue2.oper = SelOpcode.LIKE;
                //right part
                selValue2.value = new TS_Value(new TS_Union(), (short)15);
                selValue2.value.u.SetstringVal(testtype);
                aoq.condSeq[count * 2 + 2].Setvalue(selValue2);
            }

            aoq.orderBy = new SelOrder[0];
            aoq.groupBy = new AIDName[0];
            aoq.joinSeq = new JoinDef[0];

            // execute the query

            ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
            ArrayList testModeList = showTableResult_Calc(res, aIdTestMode, "Name");


            defaultList = testModeList.ToArray().Select(x => x.ToString()).Distinct().ToList();
            return defaultList;
        }


        public List<TestViewModel> GetTestNameList(string testType, List<string> projects, List<string> frameCodes, List<string> testmodes, AoSession aoSession)
        {
            List<TestViewModel> defaultList = new List<TestViewModel>();
            if (aoSession == null)
            {
                aoSession = Con_NameService();
            }

            if (testType != "--Select Test Type--")
            {

                ApplicationStructure asobj = aoSession.getApplicationStructure();
                ApplElemAccess aea = aoSession.getApplElemAccess();

                ApplicationElement aeTestType = asobj.getElementByName("Type_Of_Test");
                ApplicationElement aeProject = asobj.getElementByName("Project");
                ApplicationElement aeFrameCode = asobj.getElementByName("FrameCode");
                ApplicationElement aeTestMode = asobj.getElementByName("Subtype_Of_Test");
                ApplicationElement aeTest = asobj.getElementByName("Safety_Test");
                ////Get element Ids
                T_LONGLONG aIdTestType = aeTestType.getId();
                T_LONGLONG aIdProject = aeProject.getId();
                T_LONGLONG aIdFrameCode = aeFrameCode.getId();
                T_LONGLONG aIdTestMode = aeTestMode.getId();
                T_LONGLONG aIdTest = aeTest.getId();

                ////Construct query
                QueryStructureExt aoq = new QueryStructureExt();
                //Define attributes needed to return
                aoq.anuSeq = new SelAIDNameUnitId[2];
                aoq.anuSeq[0] = createAnu(aIdTest, "Name");
                aoq.anuSeq[1] = createAnu(aIdTest, "TestRequestId");
                if (projects.Count == 0 && frameCodes.Count == 0 && testmodes.Count == 0)
                {
                    string testtype = testType;
                    //// first condition
                    aoq.condSeq = new SelItem[1];
                    aoq.condSeq[0] = new SelItem();
                    SelValueExt selValue = new SelValueExt();
                    // left part of the condition
                    ApplicationAttribute aaRunName = aeTestType.getAttributeByName("Name");
                    selValue.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaRunName.getName()), new T_LONGLONG(0, 0));
                    // operator
                    selValue.oper = SelOpcode.LIKE;
                    //right part
                    selValue.value = new TS_Value(new TS_Union(), (short)15);
                    selValue.value.u.SetstringVal(testtype);
                    aoq.condSeq[0].Setvalue(selValue);
                }
                else if (projects.Count > 0 && frameCodes.Count == 0 && testmodes.Count == 0)
                {
                    string testtype = testType;


                    if (projects.Count == 1)
                    {
                        aoq.condSeq = new SelItem[projects.Count * 2 + 1];
                        aoq.condSeq[0] = new SelItem();
                        SelValueExt selValue = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaRunName = aeProject.getAttributeByBaseName("Name");
                        selValue.attr = new AIDNameUnitId(new AIDName(aIdProject, aaRunName.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue.oper = SelOpcode.LIKE;
                        //right part
                        selValue.value = new TS_Value(new TS_Union(), (short)15);
                        selValue.value.u.SetstringVal(projects[0]);
                        aoq.condSeq[0].Setvalue(selValue);


                        aoq.condSeq[1] = new SelItem();
                        aoq.condSeq[1].Setoperator(SelOperator.AND);

                        // third condition
                        aoq.condSeq[2] = new SelItem();
                        SelValueExt selValue2 = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                        selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue2.oper = SelOpcode.LIKE;
                        //right part
                        selValue2.value = new TS_Value(new TS_Union(), (short)15);
                        selValue2.value.u.SetstringVal(testtype);
                        aoq.condSeq[2].Setvalue(selValue2);

                    }
                    else
                    {
                        aoq.condSeq = new SelItem[projects.Count * 2 + 3];
                        int count = 0;
                        aoq.condSeq[0] = new SelItem();
                        aoq.condSeq[0].Setoperator(SelOperator.OPEN);
                        for (int i = 1; i < projects.Count + 1; i++)
                        {
                            aoq.condSeq[i + count] = new SelItem();
                            SelValueExt selValue = new SelValueExt();
                            // left part of the condition
                            ApplicationAttribute aaRunName = aeProject.getAttributeByBaseName("Name");
                            selValue.attr = new AIDNameUnitId(new AIDName(aIdProject, aaRunName.getName()), new T_LONGLONG(0, 0));
                            // operator
                            selValue.oper = SelOpcode.LIKE;
                            //right part
                            selValue.value = new TS_Value(new TS_Union(), (short)15);
                            selValue.value.u.SetstringVal(projects[i - 1]);
                            aoq.condSeq[i + count].Setvalue(selValue);

                            // second condition
                            if (i != projects.Count)
                            {
                                aoq.condSeq[i + 1 + count] = new SelItem();
                                aoq.condSeq[i + 1 + count].Setoperator(SelOperator.OR);

                            }
                            count = i;
                        }

                        aoq.condSeq[count * 2] = new SelItem();
                        aoq.condSeq[count * 2].Setoperator(SelOperator.CLOSE);

                        aoq.condSeq[count * 2 + 1] = new SelItem();
                        aoq.condSeq[count * 2 + 1].Setoperator(SelOperator.AND);

                        // third condition
                        aoq.condSeq[count * 2 + 2] = new SelItem();
                        SelValueExt selValue2 = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                        selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue2.oper = SelOpcode.LIKE;
                        //right part
                        selValue2.value = new TS_Value(new TS_Union(), (short)15);
                        selValue2.value.u.SetstringVal(testtype);
                        aoq.condSeq[count * 2 + 2].Setvalue(selValue2);
                    }

                }

                else if (projects.Count > 0 && frameCodes.Count > 0 && testmodes.Count == 0)
                {
                    string testtype = testType;


                    if (frameCodes.Count == 1)
                    {
                        aoq.condSeq = new SelItem[frameCodes.Count * 2 + 1];
                        aoq.condSeq[0] = new SelItem();
                        SelValueExt selValue = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
                        selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue.oper = SelOpcode.LIKE;
                        //right part
                        selValue.value = new TS_Value(new TS_Union(), (short)15);
                        selValue.value.u.SetstringVal(frameCodes[0]);
                        aoq.condSeq[0].Setvalue(selValue);


                        aoq.condSeq[1] = new SelItem();
                        aoq.condSeq[1].Setoperator(SelOperator.AND);

                        // third condition
                        aoq.condSeq[2] = new SelItem();
                        SelValueExt selValue2 = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                        selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue2.oper = SelOpcode.LIKE;
                        //right part
                        selValue2.value = new TS_Value(new TS_Union(), (short)15);
                        selValue2.value.u.SetstringVal(testtype);
                        aoq.condSeq[2].Setvalue(selValue2);

                    }
                    else
                    {
                        aoq.condSeq = new SelItem[frameCodes.Count * 2 + 3];
                        int count = 0;
                        aoq.condSeq[0] = new SelItem();
                        aoq.condSeq[0].Setoperator(SelOperator.OPEN);
                        for (int i = 1; i < frameCodes.Count + 1; i++)
                        {
                            aoq.condSeq[i + count] = new SelItem();
                            SelValueExt selValue = new SelValueExt();
                            // left part of the condition
                            ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
                            selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
                            // operator
                            selValue.oper = SelOpcode.LIKE;
                            //right part
                            selValue.value = new TS_Value(new TS_Union(), (short)15);
                            selValue.value.u.SetstringVal(frameCodes[i - 1]);
                            aoq.condSeq[i + count].Setvalue(selValue);

                            // second condition
                            if (i != frameCodes.Count)
                            {
                                aoq.condSeq[i + 1 + count] = new SelItem();
                                aoq.condSeq[i + 1 + count].Setoperator(SelOperator.OR);

                            }
                            count = i;
                        }
                        aoq.condSeq[count * 2] = new SelItem();
                        aoq.condSeq[count * 2].Setoperator(SelOperator.CLOSE);
                        aoq.condSeq[count * 2 + 1] = new SelItem();
                        aoq.condSeq[count * 2 + 1].Setoperator(SelOperator.AND);

                        // third condition
                        aoq.condSeq[count * 2 + 2] = new SelItem();
                        SelValueExt selValue2 = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                        selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue2.oper = SelOpcode.LIKE;
                        //right part
                        selValue2.value = new TS_Value(new TS_Union(), (short)15);
                        selValue2.value.u.SetstringVal(testtype);
                        aoq.condSeq[count * 2 + 2].Setvalue(selValue2);
                    }
                }

                else if (projects.Count > 0 && frameCodes.Count > 0 && testmodes.Count > 0)
                {
                    string testtype = testType;
                    if (testmodes.Count == 1)
                    {
                        aoq.condSeq = new SelItem[frameCodes.Count * 2 + 5];
                        int count = 0;
                        aoq.condSeq[0] = new SelItem();
                        aoq.condSeq[0].Setoperator(SelOperator.OPEN);
                        for (int i = 1; i < frameCodes.Count + 1; i++)
                        {
                            aoq.condSeq[i + count] = new SelItem();
                            SelValueExt selValue = new SelValueExt();
                            // left part of the condition
                            ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
                            selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
                            // operator
                            selValue.oper = SelOpcode.LIKE;
                            //right part
                            selValue.value = new TS_Value(new TS_Union(), (short)15);
                            selValue.value.u.SetstringVal(frameCodes[i - 1]);
                            aoq.condSeq[i + count].Setvalue(selValue);

                            // second condition
                            if (i != frameCodes.Count)
                            {
                                aoq.condSeq[i + 1 + count] = new SelItem();
                                aoq.condSeq[i + 1 + count].Setoperator(SelOperator.OR);

                            }
                            count = i;
                        }
                        aoq.condSeq[count * 2] = new SelItem();
                        aoq.condSeq[count * 2].Setoperator(SelOperator.CLOSE);
                        aoq.condSeq[count * 2 + 1] = new SelItem();
                        aoq.condSeq[count * 2 + 1].Setoperator(SelOperator.AND);

                        // third condition
                        aoq.condSeq[count * 2 + 2] = new SelItem();
                        SelValueExt selValue2 = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                        selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue2.oper = SelOpcode.LIKE;
                        //right part
                        selValue2.value = new TS_Value(new TS_Union(), (short)15);
                        selValue2.value.u.SetstringVal(testtype);
                        aoq.condSeq[count * 2 + 2].Setvalue(selValue2);

                        aoq.condSeq[count * 2 + 3] = new SelItem();
                        aoq.condSeq[count * 2 + 3].Setoperator(SelOperator.AND);

                        aoq.condSeq[count * 2 + 4] = new SelItem();
                        SelValueExt selValuetm = new SelValueExt();
                        ApplicationAttribute aaTestMode = aeTestMode.getAttributeByName("Name");
                        selValuetm.attr = new AIDNameUnitId(new AIDName(aIdTestMode, aaTestMode.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValuetm.oper = SelOpcode.EQ;
                        //right part
                        selValuetm.value = new TS_Value(new TS_Union(), (short)100);
                        selValuetm.value.u.SetstringVal(testmodes[0]);
                        aoq.condSeq[count * 2 + 4].Setvalue(selValuetm);



                    }
                    else
                    {
                        // (A Or B Or ...) And TestType And ( C Or D)
                        aoq.condSeq = new SelItem[testmodes.Count * 2 + 5 + frameCodes.Count * 2];
                        int count = 0;
                        aoq.condSeq[0] = new SelItem();
                        aoq.condSeq[0].Setoperator(SelOperator.OPEN);
                        for (int i = 1; i < testmodes.Count + 1; i++)
                        {
                            aoq.condSeq[i + count] = new SelItem();
                            SelValueExt selValue = new SelValueExt();
                            // left part of the condition
                            ApplicationAttribute aaTestMode = aeTestMode.getAttributeByName("Name");
                            selValue.attr = new AIDNameUnitId(new AIDName(aIdTestMode, aaTestMode.getName()), new T_LONGLONG(0, 0));
                            // operator
                            selValue.oper = SelOpcode.LIKE;
                            //right part
                            selValue.value = new TS_Value(new TS_Union(), (short)15);
                            selValue.value.u.SetstringVal(testmodes[i - 1]);
                            aoq.condSeq[i + count].Setvalue(selValue);

                            // second condition
                            if (i != testmodes.Count)
                            {
                                aoq.condSeq[i + 1 + count] = new SelItem();
                                aoq.condSeq[i + 1 + count].Setoperator(SelOperator.OR);

                            }
                            count = i;
                        }
                        aoq.condSeq[count * 2] = new SelItem();
                        aoq.condSeq[count * 2].Setoperator(SelOperator.CLOSE);
                        aoq.condSeq[count * 2 + 1] = new SelItem();
                        aoq.condSeq[count * 2 + 1].Setoperator(SelOperator.AND);

                        // third condition
                        aoq.condSeq[count * 2 + 2] = new SelItem();
                        SelValueExt selValue2 = new SelValueExt();
                        // left part of the condition
                        ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
                        selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
                        // operator
                        selValue2.oper = SelOpcode.LIKE;
                        //right part
                        selValue2.value = new TS_Value(new TS_Union(), (short)15);
                        selValue2.value.u.SetstringVal(testtype);
                        aoq.condSeq[count * 2 + 2].Setvalue(selValue2);


                        aoq.condSeq[count * 2 + 3] = new SelItem();
                        aoq.condSeq[count * 2 + 3].Setoperator(SelOperator.AND);

                        aoq.condSeq[count * 2 + 4] = new SelItem();
                        aoq.condSeq[count * 2 + 4].Setoperator(SelOperator.OPEN);

                        int countFc = count * 2 + 4;
                        for (int i = 1; i < frameCodes.Count + 1; i++)
                        {
                            aoq.condSeq[i + countFc] = new SelItem();
                            SelValueExt selValue = new SelValueExt();
                            // left part of the condition
                            ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
                            selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
                            // operator
                            selValue.oper = SelOpcode.LIKE;
                            //right part
                            selValue.value = new TS_Value(new TS_Union(), (short)15);
                            selValue.value.u.SetstringVal(frameCodes[i - 1]);
                            aoq.condSeq[i + countFc].Setvalue(selValue);

                            // second condition
                            if (i != frameCodes.Count)
                            {
                                aoq.condSeq[i + 1 + countFc] = new SelItem();
                                aoq.condSeq[i + 1 + countFc].Setoperator(SelOperator.OR);

                            }
                            countFc = countFc + 1;
                        }

                        aoq.condSeq[count * 2 + frameCodes.Count * 2 + 4] = new SelItem();
                        aoq.condSeq[count * 2 + frameCodes.Count * 2 + 4].Setoperator(SelOperator.CLOSE);

                    }
                }



                //else if (filterCriterias.ElementAt(2).Value != "--Select Frame Code--" && filterCriterias.ElementAt(3).Value == "--Select Test Mode--")
                //{
                //  string testtype = filterCriterias.ElementAt(0).Value;
                //  string projectName = filterCriterias.ElementAt(1).Value;
                //  string framecodeName = filterCriterias.ElementAt(2).Value;
                //  //// first condition
                //  aoq.condSeq = new SelItem[5];
                //  aoq.condSeq[0] = new SelItem();
                //  SelValueExt selValue = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaRunName = aeTestType.getAttributeByName("Name");
                //  selValue.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaRunName.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue.value.u.SetstringVal(testtype);
                //  aoq.condSeq[0].Setvalue(selValue);

                //  // second condition
                //  aoq.condSeq[1] = new SelItem();
                //  aoq.condSeq[1].Setoperator(SelOperator.AND);
                //  // third condition
                //  aoq.condSeq[2] = new SelItem();
                //  SelValueExt selValue2 = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaProject = aeProject.getAttributeByBaseName("Name");
                //  selValue2.attr = new AIDNameUnitId(new AIDName(aIdProject, aaProject.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue2.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue2.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue2.value.u.SetstringVal(projectName);
                //  aoq.condSeq[2].Setvalue(selValue2);

                //  // fourth condition
                //  aoq.condSeq[3] = new SelItem();
                //  aoq.condSeq[3].Setoperator(SelOperator.AND);
                //  // fifth condition
                //  aoq.condSeq[4] = new SelItem();
                //  SelValueExt selValue4 = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaFrameCode = aeFrameCode.getAttributeByName("FrameCode");
                //  selValue4.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaFrameCode.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue4.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue4.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue4.value.u.SetstringVal(framecodeName);
                //  aoq.condSeq[4].Setvalue(selValue4);

                //}

                //else if (filterCriterias.ElementAt(3).Value != "--Select Test Mode--")
                //{
                //  string testtype = filterCriterias.ElementAt(0).Value;
                //  string projectName = filterCriterias.ElementAt(1).Value;
                //  string framecodeName = filterCriterias.ElementAt(2).Value;
                //  string testmodeName = filterCriterias.ElementAt(3).Value;
                //  //// first condition
                //  aoq.condSeq = new SelItem[7];
                //  aoq.condSeq[0] = new SelItem();
                //  SelValueExt selValue = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaRunName = aeTestType.getAttributeByName("Name");
                //  selValue.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaRunName.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue.value.u.SetstringVal(testtype);
                //  aoq.condSeq[0].Setvalue(selValue);

                //  // second condition
                //  aoq.condSeq[1] = new SelItem();
                //  aoq.condSeq[1].Setoperator(SelOperator.AND);
                //  // third condition
                //  aoq.condSeq[2] = new SelItem();
                //  SelValueExt selValue2 = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaProject = aeProject.getAttributeByBaseName("Name");
                //  selValue2.attr = new AIDNameUnitId(new AIDName(aIdProject, aaProject.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue2.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue2.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue2.value.u.SetstringVal(projectName);
                //  aoq.condSeq[2].Setvalue(selValue2);

                //  // fourth condition
                //  aoq.condSeq[3] = new SelItem();
                //  aoq.condSeq[3].Setoperator(SelOperator.AND);
                //  // fifth condition
                //  aoq.condSeq[4] = new SelItem();
                //  SelValueExt selValue4 = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaFrameCode = aeFrameCode.getAttributeByName("FrameCode");
                //  selValue4.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaFrameCode.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue4.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue4.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue4.value.u.SetstringVal(framecodeName);
                //  aoq.condSeq[4].Setvalue(selValue4);

                //  // sixth condition
                //  aoq.condSeq[5] = new SelItem();
                //  aoq.condSeq[5].Setoperator(SelOperator.AND);
                //  // seventh condition
                //  aoq.condSeq[6] = new SelItem();
                //  SelValueExt selValue6 = new SelValueExt();
                //  // left part of the condition
                //  ApplicationAttribute aaTestMode = aeTestMode.getAttributeByName("Name");
                //  selValue6.attr = new AIDNameUnitId(new AIDName(aIdTestMode, aaTestMode.getName()), new T_LONGLONG(0, 0));
                //  // operator
                //  selValue6.oper = SelOpcode.LIKE;
                //  //right part
                //  selValue6.value = new TS_Value(new TS_Union(), (short)15);
                //  selValue6.value.u.SetstringVal(testmodeName);
                //  aoq.condSeq[6].Setvalue(selValue6);

                //}

                // initilize other parts
                aoq.orderBy = new SelOrder[0];
                aoq.groupBy = new AIDName[0];
                aoq.joinSeq = new JoinDef[0];
                //// execute the query
                ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
                ArrayList testNameList = showTableResult_Calc(res, aIdTest, "Name");
                ArrayList testrequestIdList = showTableResult_Calc(res, aIdTest, "TestRequestId");
                List<TestViewModel> testList = new List<TestViewModel>();

                for (int i = 0; i < testNameList.ToArray().Count(); i++)
                {
                    var testViewModel = new TestViewModel
                    {
                        TestName = testNameList.ToArray()[i].ToString(),
                        TestrequestId = (int)testrequestIdList.ToArray()[i]
                    };
                    testList.Add(testViewModel);
                }

                return testList;

            }


            ////aoq.anuSeq[1] = createAnu(aIdTest, "TestRequestId");
            ////Define conditions for the query
            //// first condition
            //aoq.condSeq = new SelItem[3];
            //aoq.condSeq[0] = new SelItem();

            //SelValueExt selValue = new SelValueExt();
            //// left part of the condition
            //ApplicationAttribute aaRunName = aeFrameCode.getAttributeByName("FrameCode");
            //selValue.attr = new AIDNameUnitId(new AIDName(aIdFrameCode, aaRunName.getName()), new T_LONGLONG(0, 0));
            //// operator
            //selValue.oper = SelOpcode.LIKE;
            ////right part
            //selValue.value = new TS_Value(new TS_Union(), (short)15);
            //selValue.value.u.SetstringVal(frameCodeName);
            //aoq.condSeq[0].Setvalue(selValue);

            //// second condition
            //aoq.condSeq[1] = new SelItem();
            //aoq.condSeq[1].Setoperator(SelOperator.AND);
            //// third condition
            //aoq.condSeq[2] = new SelItem();
            //SelValueExt selValue2 = new SelValueExt();
            //// left part of the condition
            //ApplicationAttribute aaTestType = aeTestType.getAttributeByBaseName("Name");
            //selValue2.attr = new AIDNameUnitId(new AIDName(aIdTestType, aaTestType.getName()), new T_LONGLONG(0, 0));
            //// operator
            //selValue2.oper = SelOpcode.LIKE;
            ////right part
            //selValue2.value = new TS_Value(new TS_Union(), (short)15);
            //selValue2.value.u.SetstringVal(testtype);
            //aoq.condSeq[2].Setvalue(selValue2);
            //// initilize other parts
            //aoq.orderBy = new SelOrder[0];
            //aoq.groupBy = new AIDName[0];
            //aoq.joinSeq = new JoinDef[0];

            //// execute the query

            //ResultSetExt[] res = aea.getInstancesExt(aoq, 1000);
            //ArrayList testModeList = showTableResult_Calc(res, aIdTestMode, "Name");


            //defaultList = testModeList.ToArray().Select(x => x.ToString()).Distinct().ToList();
            return defaultList;
        }

        public static SelAIDNameUnitId createAnu(T_LONGLONG aid, String aaName)
        {
            SelAIDNameUnitId anu = new SelAIDNameUnitId();
            anu.aggregate = AggrFunc.NONE;
            anu.attr = new AIDName(aid, aaName);
            anu.unitId = new T_LONGLONG(0, 0);
            return (anu);
        }
    }
}
