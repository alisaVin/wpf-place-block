using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Windows;
using Teigha.DatabaseServices;
using Teigha.Runtime;

namespace place_block_wpf_ares.src.Model
{
    public class DemandLoadingService
    {
        public static void RegisterForDemandLoading()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string name = assembly.GetName().Name;
            string path = assembly.Location;

            List<string> globCmds = new List<string>();
            List<string> locCmds = new List<string>();
            List<string> groups = new List<string>();

            System.Reflection.Module[] modules = assembly.GetModules(true);
            foreach (System.Reflection.Module mod in modules)
            {
                Type[] types = mod.GetTypes();
                foreach (Type type in types)
                {
                    ResourceManager rm = new ResourceManager(type.FullName, assembly);
                    rm.IgnoreCase = true;

                    MethodInfo[] meths = type.GetMethods();
                    foreach (MethodInfo meth in meths)
                    {
                        object[] attrs = meth.GetCustomAttributes(typeof(CommandMethodAttribute), true);
                        foreach (object attr in attrs)
                        {
                            CommandMethodAttribute cmdAtt = attr as CommandMethodAttribute;
                            if (cmdAtt != null)
                            {
                                string globName = cmdAtt.GlobalName;
                                string locName = cmdAtt.GlobalName;
                                string lid = cmdAtt.LocalizedNameId;

                                if (lid != null)
                                {
                                    try
                                    {
                                        locName = rm.GetString(lid);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        MessageBox.Show("Lokale Name aus dem Resource Manager kann nicht zugewiesen werden:\n" + ex.Message + "\n" + ex.StackTrace);
                                    }
                                }
                                globCmds.Add(globName);
                                globCmds.Add(locName);

                                if (cmdAtt.GroupName != null && !groups.Contains(cmdAtt.GroupName))
                                    groups.Add(cmdAtt.GroupName);
                            }
                        }
                    }
                }
            }
            //int flags = (globCmds.Count > 0 ? 12 : 2); ---> for later
            int flags = 2; //---> load on AutoCAD startup

            CreateDemandLoadingEntries(name, path, globCmds, locCmds, groups, flags, true);
        }

        public static void UnregisterForDemandLoading()
        {
            RemoveDemandLoadingEntries(true);
        }

        private static void CreateDemandLoadingEntries(
            string name,
            string path,
            List<string> globCmds,
            List<string> locCmds,
            List<string> groups,
            int flags,
            bool currentUser)
        {
            Microsoft.Win32.RegistryKey hive = (currentUser ? Microsoft.Win32.Registry.CurrentUser : Microsoft.Win32.Registry.LocalMachine);

            Microsoft.Win32.RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.UserRegistryProductRootKey, true);
            using (ack)
            {
                Microsoft.Win32.RegistryKey appk = ack.CreateSubKey("Applications");
                using (appk)
                {
                    string[] subKeys = appk.GetSubKeyNames();
                    foreach (string subKey in subKeys)
                    {
                        if (subKey.Equals(name))
                            return;
                    }

                    Microsoft.Win32.RegistryKey rk = appk.CreateSubKey(name);
                    using (rk)
                    {
                        rk.SetValue("DESCRIPTION", name, Microsoft.Win32.RegistryValueKind.String);
                        rk.SetValue("LOADCTRLS", flags, Microsoft.Win32.RegistryValueKind.DWord);
                        rk.SetValue("LOADER", path, Microsoft.Win32.RegistryValueKind.String);
                        rk.SetValue("MANAGED", 1, Microsoft.Win32.RegistryValueKind.DWord);

                        if ((globCmds.Count == locCmds.Count) && globCmds.Count > 0)
                        {
                            Microsoft.Win32.RegistryKey ck = rk.CreateSubKey("Commands");
                            using (ck)
                            {
                                for (int i = 0; i < globCmds.Count; i++)
                                {
                                    ck.SetValue(globCmds[i], locCmds[i], Microsoft.Win32.RegistryValueKind.String);
                                }
                            }
                        }
                        if (groups.Count > 0)
                        {
                            Microsoft.Win32.RegistryKey gk = rk.CreateSubKey("Groups");
                            using (gk)
                            {
                                foreach (var groupName in groups)
                                {
                                    gk.SetValue(groupName, groupName, Microsoft.Win32.RegistryValueKind.String);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RemoveDemandLoadingEntries(bool currentUser)
        {
            try
            {
                Microsoft.Win32.RegistryKey hive = (currentUser ? Microsoft.Win32.Registry.CurrentUser : Microsoft.Win32.Registry.LocalMachine);
                Microsoft.Win32.RegistryKey ack = hive.OpenSubKey(HostApplicationServices.Current.UserRegistryProductRootKey);
                using (ack)
                {
                    Microsoft.Win32.RegistryKey appk = ack.OpenSubKey("Applications", true);
                    using (appk)
                    {
                        appk.DeleteSubKeyTree(Assembly.GetExecutingAssembly().GetName().Name);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Fehler bei der Umregistrierung:\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
