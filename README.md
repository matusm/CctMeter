CctMeter - A Logger for Spectroradiometers
==========================================

## Overview

A standalone command line app controlling the portable spectroradiometers MSC15 and CSS-45 by [Gigahertz-Optik GmbH](https://www.gigahertz-optik.com/) via its USB interface.

Its main usage is to correlated color temperature (CCT) measurements of light sources. This process can be triggered manually whenever needed. A timestamp, average value, dispersion parameters and the corresponding photometric quantity values are logged in a file.

## Command Line Usage

```
CctMeter [options]
```

### Options

`--comment` : User supplied string to be included in the log file metadata.

`--number (-n)` : Number of samples per run.

`--logfile` : Log file name.

`--skipdark (-s)` : Skip dark offset measurement at startup.

### Examples

```
CctMeter -n20 --comment="my annotation"
```
Use 20 samples per measurement and write a comment in the log file. The log file has the default name `cctmeter.log` and is located in the current working directory.

```
CctMeter --logfile="C:\temp\MyLogFile.txt"
```
Use 10 samples per measurement (default). The full path of the log file is given. If the directory `C:\temp` does not exist, the programm will crash.


## Log File Entries

On app start the identifications of the instrument is queried and logged.

* The average CCT value and its standard deviation, in K.                 

* The average illuminance and its standard deviation, in lx.

* Sensor specific parameters like the internal temperature in °C and the integration time in s.


## Dependencies

Bev.Instruments.Msc15: https://github.com/matusm/Bev.Instruments.Msc15

At.Matus.StatisticPod: https://github.com/matusm/At.Matus.StatisticPod

CommandLineParser: https://github.com/commandlineparser/commandline 
