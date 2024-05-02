using System;
using System.Runtime.InteropServices;
using ArduinoUploader;
using ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages;
using CommandLine;
using Microsoft.Extensions.Logging;
using NLog;

namespace ArduinoSketchUploader
{
    /// <summary>
    /// The ArduinoSketchUploader can upload a compiled (Intel) HEX file directly to an attached Arduino.
    /// </summary>
    internal class Program
    {
        private class ArduinoSketchUploaderLogger : IArduinoUploaderLogger
        {
            private static readonly Logger Logger = LogManager.GetLogger("ArduinoSketchUploader");

            public void Error(string message, Exception exception)
            {
                Logger.Error(exception, message);
            }

            public void Warn(string message)
            {
                Logger.Warn(message);
            }

            public void Info(string message)
            {
                Logger.Info(message);
            }

            public void Debug(string message)
            {
                Logger.Debug(message);
            }

            public void Trace(string message)
            {
                Logger.Trace(message);
            }
        }

        private static int doUpload(CommandLineOptions commandLineOptions)
        {
            var logger = new ArduinoSketchUploaderLogger();
            var options = new ArduinoSketchUploaderOptions
            {
                PortName = commandLineOptions.PortName,
                FileName = commandLineOptions.FileName,
                ArduinoModel = commandLineOptions.ArduinoModel
            };

            var progress = new Progress<double>(
                p => logger.Info($"Upload progress: {p * 100:F1}% ..."));

            var uploader = new ArduinoUploader.ArduinoSketchUploader(options, logger, progress);
            try
            {
                uploader.UploadSketch();
                return (int)StatusCodes.Success;
            }
            catch (ArduinoUploaderException)
            {
                return (int)StatusCodes.ArduinoUploaderException;
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected exception: {ex.Message}!", ex);
                return (int)StatusCodes.GeneralRuntimeException;
            }
        }
        private static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(commandLineOptions => Environment.Exit(doUpload(commandLineOptions)));
        }

        private enum StatusCodes
        {
            Success,
            ArduinoUploaderException,
            GeneralRuntimeException
        }
    }
}