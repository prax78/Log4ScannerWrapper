# Log4ScannerWrapper

** Log4ScannerWrapper [Windows only]

    This is a C# wrapper for the Log4j2Scan tool that can be found here https://github.com/logpresso/CVE-2021-44228-Scanner/
    Log4J2-Scan is an executable from the LogPresso that helps you in finding Log4J vulnerable files on servers.
    Log4J2-Scan  is a standalone executable, so Windows Server Admin has to take this to multiple servers to scan Windows servers for the vulnerable log4J files on it.
      To run it on multiple servers from  one source Windows servers, I have created this tool so that you can run Log4J2-Scan tool on multiple servers and collect data.
      
       How To use this to run on multiple servers:
        1 Download log4j2-scan 2.3.2 (Windows x64, 7z) from https://github.com/logpresso/CVE-2021-44228-Scanner.
        2 Download Log4JScannerWrapper.zip from https://github.com/prax78/Log4ScannerWrapper/blob/master/Log4ScannerWrapper.zip.
        3 This command has 4 Arguements Refer Screenshot.
            1 Log4j2-SCAN.exe complete path that was downloaded in the step 1.
            2 Log4J2-SCAN.EXE arguements like "--all-drives" this will scan all the drives on a server.
            3 Output location, this must be a common share to collect scans output from remote servers.
            4 Server list text file, this is must as this will have remote server names.
        
        4 Thats it!! all done, now just collect scans output from remote servers as you have given in the Step 3.
        
        ** Note If you get VCRUNTIME140.dll not found error, install Visual C++ Redistributable. as per LogPresso Github
 ![alt text](https://github.com/prax78/Log4ScannerWrapper/blob/master/log4jscanner1.PNG)
        ** Command Mandatory Parameters

        
        ** First this will copy the standalone scanner from Logpresso to multiple servers and then PS Invoke-Command to run the same on target servers as Job.
           You can watch job status runing in parallel as shown below.
![alt text](https://github.com/prax78/Log4ScannerWrapper/blob/master/log4jscanner2.PNG)
           
        ** Finished Screen, once it is finished collect it from the given Output location.
![alt text](https://github.com/prax78/Log4ScannerWrapper/blob/master/log4jscanner3.PNG)
        
        ** Output Location Sample
![alt text](https://github.com/prax78/Log4ScannerWrapper/blob/master/log4jscanner4.PNG)
        
        ** Sample server.txt file with target server names.
![alt text](https://github.com/prax78/Log4ScannerWrapper/blob/master/log4jscanner5.PNG)
        
