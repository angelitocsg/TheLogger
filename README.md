# TheLogger
Log library to many information level, like Critical, Error, Debug etc.

## Possible types of logs

```
LogType.Critical = 1
LogType.Error = 2
LogType.Info = 3
LogType.Warning = 4
LogType.Debug = 5
```

## Application type

```
AppType.None = 0
AppType.Console = 1
```

## Sintax

### General write log commands

Setting only the param `message`:
```csharp
TheLogger.Log.Write(message);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Info, message);
```

Setting params `logType` and `message`:
```csharp
TheLogger.Log.Write(logType, message);
```

Setting param as `Exception`:
```csharp
TheLogger.Log.Write(exception);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Critial, ex.Message);
// TheLogger.Log.Write(LogType.Debug, ex.StackTrace);
// # If `forceCloseOnError` is `true` will also run:
// Write(LogType.Critical, "Application exit code 99");
// Environment.Exit(99);
```

### Write log by types

```csharp
TheLogger.Log.Critical(message);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Critical, message);

TheLogger.Log.Error(message);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Error, message);

TheLogger.Log.Info(message);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Info, message);

TheLogger.Log.Warning(message);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Warning, message);

TheLogger.Log.Debug(message);
// # Will execute internally:
// TheLogger.Log.Write(LogType.Debug, message);
```

### Optional Startup Setup

```csharp
// # Default values
// fileName = log.txt
// filePath =  AppDomain.CurrentDomain.BaseDirectory
// logLevel = LogType.Info (the same or lower levels will be written to the log file)
// appType = AppType.None (if AppType.Console, the log will be written to the log file and the console app)
// forceCloseOnError = false (on Exception in the Log class, the application will be ended)
TheLogger.Log.Setup(fileName, filePath, logLevel, appType, forceCloseOnError);
```

### Additional Functions

Get a instance of `Log` for use with _Extensions Methods_:
```csharp
Log.er.MyExtensionMethod();
```

Read last `n` lines of the current log file:
```csharp
String lastLines = TheLogger.Log.Read(numLines);
```

Read last `n` lines of an `StreamReader` object:
```csharp
String[] lastLines = TheLogger.Log.Tail(streamReader, lines);
```

Simulate a PHP `print_r`:
```csharp
String value = Print.R(object, 0);
```
