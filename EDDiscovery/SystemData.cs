﻿using EDDiscovery.DB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EDDiscovery
{
    public enum SystemInfoSource
    {
        RW = 1,
        EDSC = 2,
        EDDB = 4,
        Inhumierer = 5

    }



    public class SystemData
    {
        static JArray systems;
        //static List<SystemClass> listSystems = new List<SystemClass>();

        public SystemData()
        {
            //ReadData();V:\RobertW\EDDiscovery\EDDiscovery\systems.json
        }

        //public bool ReadData()
        //{
        //    using (StreamReader reader = File.OpenText(@"systems.json"))
        //    {
        //        string str = reader.ReadToEnd();
        //        systems = (JArray)JArray.Parse(str);

        //        listSystems = new List<SystemClass>();

        //        foreach (JObject jo in systems)
        //        {
        //            listSystems.Add(new SystemClass(jo, SystemInfoSource.RW));
        //        }

        //    }

        //    return true;
        //}



        //public bool ParseEDSCObsolete(string json)
        //{
        //    JObject edsc = null;
        //    if (json!=null)
        //        edsc = (JObject)JObject.Parse(json);

        //    listSystems = new List<SystemClass>();

        //    if (edsc == null)
        //        return false;

        //    JObject edscdata = (JObject)edsc["d"];
        //    JArray systems = (JArray)edscdata["systems"];

        //    foreach (JObject jo in systems)
        //    {
        //        SystemClass system = new SystemClass(jo, SystemInfoSource.EDSC);
                
        //  //      if (system.HasCoordinate)
        //  //          listSystems.Add(system);
        //    }
        //    return true;
        //}

        static public List<SystemClass> SystemList
        {
            get
            {
                return SQLiteDBClass.globalSystems;
            }
        }

        public int Count
        {
            get
            {
                if (systems != null)
                    return systems.Count;

                return 0;
            }
        }

        static public SystemClass GetSystem(string name)
        {
            if (name==null)
                return null;

            string lname = name.ToLower();


            if (SQLiteDBClass.dictSystems.ContainsKey(lname))
                return SQLiteDBClass.dictSystems[lname];
            else 
                return null;
        }


        static public SystemClass GetSystemOld(string name)
        {
            if (name == null)
                return null;

            string lname = name.ToLower();

            
            var obj = from p in SystemList where p.SearchName == lname select p;

            if (obj.Count() < 1)
                return null;

            return (SystemClass)obj.First();

            return SQLiteDBClass.dictSystems[lname];
        }


        public static double Distance(SystemClass s1, SystemClass s2)
        {
            if (s1 == null || s2== null)
                return -1;

            //return Math.Sqrt(Math.Pow(s1.x - s2.x, 2) + Math.Pow(s1.y - s2.y, 2) + Math.Pow(s1.z - s2.z, 2));
            return Math.Sqrt((s1.x - s2.x) * (s1.x - s2.x) + (s1.y - s2.y) * (s1.y - s2.y) + (s1.z - s2.z) * (s1.z - s2.z));
        }

        public static double DistanceX2(SystemClass s1, SystemClass s2)
        {
            if (s1 == null || s2 == null)
                return -1;

            //return Math.Sqrt(Math.Pow(s1.x - s2.x, 2) + Math.Pow(s1.y - s2.y, 2) + Math.Pow(s1.z - s2.z, 2));
            return ((s1.x - s2.x) * (s1.x - s2.x) + (s1.y - s2.y) * (s1.y - s2.y) + (s1.z - s2.z) * (s1.z - s2.z));
        }

        public static double Distance(string s1, string s2)
        {
            return Distance(GetSystem(s1), GetSystem(s2));
        }


    }
}
