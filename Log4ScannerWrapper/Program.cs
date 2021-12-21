using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;

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
                    InvokePSCommand(exeFile, arguements, outputFile, System.IO.File.ReadAllLines(computerList)); 
                   
                    
                }
                
                
            

            }

            else
            {
                Console.WriteLine("Mandatory Parametters Missing \nLong4ScanWarpper.exe c:\\log4j2-scan.exe location arguements OutputfileLocation ComputerList\nExammple\nlog4j2-scan.exe c:\\log4j2-scan.exe --all-drives c:\\temp c:\\serverlist.txt");
            }
        }
        static void CopyExeFileToRemoteServer(String exe, String computers)
        {
            if(File.Exists("Log4ScannerWrapper.log") )
            {
                File.Delete("Log4ScannerWrapper.log");
            }


            if (System.IO.File.Exists(computers))
            { 
                
                foreach (var comp in System.IO.File.ReadAllLines(computers))
                {
                    Console.WriteLine($"Copying {exe} to {comp}\n\n");
                    
                    if (System.IO.Directory.Exists($"\\\\{comp}\\c$\\temp"))
                    {
                        try { System.IO.File.Copy(exe, $"\\\\{comp}\\c$\\temp\\log4j2-scan.exe", true);
                            //File.AppendAllText("Log4ScannerWrapper.log", $"Copying EXE File to {comp}");
                            //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);

                        }
                        catch (Exception ex) { 
                            //File.AppendAllText("Log4ScannerWrapper.log", $"Copying EXE File to {comp} FAILED");
                            //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);
                            Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"ERROR {ex.Message}"); Console.ForegroundColor = ConsoleColor.White;
                        }

                    }
                   else
                        {
                        try
                        {
                          
                          
                            System.IO.Directory.CreateDirectory($"\\\\{comp}\\c$\\temp");

                          
                            System.IO.File.Copy(exe, $"\\\\{comp}\\c$\\temp\\log4j2-scan.exe", true);
                            //File.AppendAllText("Log4ScannerWrapper.log", $"Copying EXE File to {comp}");
                            //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);

                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            System.Console.WriteLine($"{e.Message}"); Console.ForegroundColor = ConsoleColor.White; ;
                            //File.AppendAllText("Log4ScannerWrapper.log", $"Copying EXE File to {comp} FAILED");
                            //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);
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

               
                
                PSDataCollection<PSObject> jobCol = new PSDataCollection<PSObject>();
                PSDataCollection<PSObject> checkjobCol = new PSDataCollection<PSObject>();
                Console.WriteLine(Environment.NewLine);
                foreach (var computername in computers)
                {
                    var comd = $"c:\\temp\\log4j2-scan.exe {args} > c:\\temp\\{computername}.txt";
                    Console.WriteLine($"Executing {comd}\n\n");
                    //File.AppendAllText("Log4ScannerWrapper.log", $"EXECUTING {computername}");
                    //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);
                    var myscript = ScriptBlock.Create(comd);
                    Console.WriteLine(computername);
                    using (PowerShell invokePS = PowerShell.Create().AddCommand("Invoke-Command").AddParameter("ScriptBlock", myscript).AddParameter("ComputerName", computername).AddParameter("AsJob"))
                    {
                       invokePS.Runspace = rs;


                        try
                        {
                            IAsyncResult asyncResult = invokePS.BeginInvoke<PSObject, PSObject>(null, jobCol);
                            asyncResult.AsyncWaitHandle.WaitOne();
                        }
                        catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Red;Console.WriteLine($"{ex.Message} ");Console.ForegroundColor = ConsoleColor.White;
                            
                        }

                    }
                    
                }
               

                while (jobCol.Any(item=>item.Members["JobStateInfo"].Value.ToString()=="Running"))
                {
                    foreach (var job in jobCol)
                    {
                        Console.WriteLine($"Job ID--> {job.Members["Id"].Value} :: Job State--> {job.Members["JobStateInfo"].Value} :: Server--> {job.Members["Location"].Value}\n");
                       
                    }
                    System.Threading.Thread.Sleep(3000);
                    Console.Clear();
                }

                Console.WriteLine(Environment.NewLine);
                foreach(var job in jobCol)
                {

                    using (PowerShell FailedServesJob = PowerShell.Create().AddCommand("Get-Job").AddParameter("Id", job.Members["Id"].Value))
                    {
                        FailedServesJob.Runspace = rs;
                        try
                        {
                            IAsyncResult failed = FailedServesJob.BeginInvoke<PSObject, PSObject>(null, checkjobCol);
                            failed.AsyncWaitHandle.WaitOne();
                        }
                        catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Red;Console.WriteLine($"ERROR {ex.Message}");Console.ForegroundColor = ConsoleColor.White; }
                    }

                }
                GetResultData(output, computers);
                Console.Clear();
                foreach(var job in checkjobCol)
                {
                    if(job.Members["JobStateInfo"].Value.ToString()=="Failed")
                    {Console.ForegroundColor=ConsoleColor.Red; Console.WriteLine($"Failed {job.Members["Location"].Value.ToString()} Either WINRM Not Running or Missing VC++ runtime or Server Down"); Console.ResetColor();

                        //File.AppendAllText("Log4ScannerWrapper.log", $"Failed {job.Members["Location"].Value.ToString()}");
                        //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);
                    }

                    
                    
                }
                Console.WriteLine(Environment.NewLine);

                
                Console.WriteLine($"All done Thank you, collect your output {output} ");
               

               


            }
        }

        static void GetResultData(string outputdata,string[] computers)
        {
            foreach(var computer in computers)
            {
                try
                {
                    Console.WriteLine($"Fetching output file from {computer}");
                    System.IO.File.Copy($"\\\\{computer}\\c$\\temp\\{computer}.txt", $"{outputdata}\\{computer}.txt",true);
                    //File.AppendAllText("Log4ScannerWrapper.log", $"Copying \\\\{computer}\\c$\\temp\\{computer}.txt to {outputdata} ");
                    //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);
                }
                catch (Exception ee) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"ERROR {ee.Message}"); Console.ForegroundColor = ConsoleColor.White;
                    //File.AppendAllText("Log4ScannerWrapper.log", $"{ee.Message} {computer}");
                    //File.AppendAllText("Log4ScannerWrapper.log", Environment.NewLine);
                }   
            }
        }
    }
}
