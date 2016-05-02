#data-collection-service-for-pi-system

This project is a prototype built intitially to read data from the **Fitbit API** Web Service. It was completely refactored and is now built with a plugin model so more data sources can be supported.

The FitBit data collector have not been updated after the refactoring so code is commented out.  If you need it you could use commented code to rewrite it in the new way.

# How it works?

**Plugins**  
You should look at the existings plugins to know how to implement yours.  Settings can be passed to the data collector by using JSON configuration.  And current GitHub data collector also uses configuration from AF Elements.  This is quite useful to use AF Configuration as it can be protected with appropriate credentials (e.g. to protect your application API key, and having it not published in the settings file on GitHub.)

**Data Collection**  
The concept is really basic, there are 3 separate threads running in a producer/consumer fashion:
+ Configuration manager
+ data collectors Manager
+ Data Writer

The code is not yet fully optimized, configuration manager refresh has yet to be worked on.
But it implements some strategies for better performances with the PI System. ( bulk load of elements, time ordered and bulk writes)


#What is in the solution? 

+ The business code logic needs to be written in the Core dll.

+ Then use the command line and service code to write the code to call business code from the dll.

+ After compiling:  use the /Build folder in the first level directory level to test the application.


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
