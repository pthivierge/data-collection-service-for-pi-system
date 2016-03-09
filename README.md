#web-service-reader-for-pi-system

This project is a prototype built specifically to read data from the **Fitbit API**.
It might evolve and provide a basic base of work that you could extent with your own APIs/Web Services.  
However, the project is not there yet.  

To get data from FitBit, this project makes use of the FitBit.Net library.  This simplifies a lot the data retrieval from FitBit.

# How it works?

The concept is really basic, there are 3 separate threads running in a producer/consumer fashion:
+ Configuration manager
+ data collectors Manager
+ Data Writer

The code is not yet fully optimized, configuration manager refresh has yet to be worked on.
But it implements some strategies for better performances with the PI System. ( bulk load of elements, time ordered and bulk writes)


#What is in the solution? 

+ The business code logic needs to be written in the Core dll.

+ Then use the command line and service code to write the code to call business code from the dll.

+ After compiling:  use the /Build folder in the first level directory level to test the application. you may simply open a command line from there and use the FDS.CommandLine.exe.


#License
 
    Copyright 2015 Patrice Thivierge Fortin
 
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
 
    http://www.apache.org/licenses/LICENSE-2.0
 
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
