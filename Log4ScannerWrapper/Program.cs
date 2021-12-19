using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Log4ScannerWrapper
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if (args.Count() == 4)
            {
                String exeFile = args[0];
                String arguements = args[1];
                String outputFile = args[2];
                String computerList=args[3];
                if (System.IO.File.Exists(exeFile) && System.IO.File.Exists(computerList))
                {
                    CopyExeFileToRemoteServer(exeFile, computerList);
                    try { InvokePSCommand(exeFile, arguements, outputFile, System.IO.File.ReadAllLines(computerList)); }
                    catch (AggregateException ex){ Console.WriteLine(ex.Message); }
                    
                }
                
                
            

            }

            else
            {
                Console.WriteLine("Mandatory Parametters Missing \nLong4ScanWarpper.exe c:\\log4j2-scan.exe location arguements OutputfileLocation ComputerList\nExammple\nlog4j2-scan.exe c:\\log4j2-scan.exe --all-drives c:\\temp c:\\serverlist.txt");
            }
        }
        static void CopyExeFileToRemoteServer(String exe, String computers)
        {


            if(System.IO.File.Exists(computers))
            { 
                bool exist=false;
                foreach (var comp in System.IO.File.ReadAllLines(computers))
                {
                    Console.WriteLine($"Copying {exe} to {comp}");
                    try {  exist = System.IO.Directory.Exists($"\\\\{comp}\\c$\\temp");
                     
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    
                    if (exist)
                    {
                        try { System.IO.File.Copy(exe, $"\\\\{comp}\\c$\\temp\\log4j2-scan.exe", true);  }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }

                    }
                   else
                        {
                        try
                        {
                          
                          
                            System.IO.Directory.CreateDirectory($"\\\\{comp}\\c$\\temp");

                          
                            System.IO.File.Copy(exe, $"\\\\{comp}\\c$\\temp\\log4j2-scan.exe", true);

                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e.Message);
                        }

                        }
                    }
                
            }
            else
            {
                Console.WriteLine($"{computers} File does not exist");
            }
            

            
        }
        static void InvokePSCommand(String exe, String args, String output, String[] computers)
        {
            using (Runspace rs = RunspaceFactory.CreateRunspace())
            {
                rs.Open();

                var comd = $"c:\\temp\\log4j2-scan.exe {args} |out-file {output}\\{Environment.MachineName}.txt";
                Console.WriteLine(comd);
                var myscript = ScriptBlock.Create(comd);
                PSDataCollection<PSObject> jobCol = new PSDataCollection<PSObject>();
                PSDataCollection<PSObject> checkjobCol = new PSDataCollection<PSObject>();
         
                foreach (var computername in computers)
                {
                    Console.WriteLine(computername);
                    using (PowerShell invokePS = PowerShell.Create().AddCommand("Invoke-Command").AddParameter("ScriptBlock", myscript).AddParameter("ComputerName", computername).AddParameter("AsJob"))
                    {
                       invokePS.Runspace = rs;



                        IAsyncResult asyncResult  = invokePS.BeginInvoke<PSObject,PSObject>(null, jobCol);
                       asyncResult.AsyncWaitHandle.WaitOne();

                    }
                    
                }
                
                
               while(jobCol.Any(item=>item.Members["JobStateInfo"].Value.ToString()=="Running"))
                {
                    foreach (var job in jobCol)
                    {
                        Console.WriteLine($"Job ID--> {job.Members["Id"].Value} Job State--> {job.Members["JobStateInfo"].Value} Server--> {job.Members["Location"].Value}");
                       
                    }
                    System.Threading.Thread.Sleep(1000);
                    Console.Clear();
                }

                Console.WriteLine($"All done Thank you, collect your output {output} ");

               


            }
        }
    }
}
