using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;

namespace SimpleFileLogger.Tests
{
    public class FileLoggerTests
    {
        
        [Fact]
        public void BasicLogFolderTest(){

            var providerMock = new Mock<IFileLoggerProvider>();
            providerMock.SetupGet(p => p.LogFolder).Returns("logs");
            providerMock.SetupGet(p => p.EventOptionsDict).Returns( new Dictionary<int, EventOptions>());
            var logger = new FileLogger(providerMock.Object, "log1");

            logger.Log<object>(LogLevel.Critical, 0, new object(), null, (o,e) => "" );

            var expectedFilePath = $"logs/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath)), Times.Once);
        }

        [Fact]
        public void NoRootLogFolderTest(){

            var providerMock = new Mock<IFileLoggerProvider>();
            providerMock.SetupGet(p => p.LogFolder).Returns("");
            providerMock.SetupGet(p => p.EventOptionsDict).Returns( new Dictionary<int, EventOptions>());
            var logger = new FileLogger(providerMock.Object, "log1");

            logger.Log<object>(LogLevel.Critical, 0, new object(), null, (o,e) => "" );

            var expectedFilePath = $"log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath)), Times.Once);
        }

        [Fact]
        public void FileNameExtensionTest(){

            var providerMock = new Mock<IFileLoggerProvider>();
            providerMock.SetupGet(p => p.LogFolder).Returns("logs");
            providerMock.SetupGet(p => p.EventOptionsDict).Returns( new Dictionary<int, EventOptions>()
            {
                {1, new EventOptions{ Id=1, NameExtension = "Ext"}}
            });
            var logger = new FileLogger(providerMock.Object, "log1");

            logger.Log<object>(LogLevel.Critical, 1, new object(), null, (o,e) => "" );

            var expectedFilePath = $"logs/log1Ext_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath)), Times.Once);
        }

        [Fact]
        public void FileNameExtensionFromEventTest(){

            var providerMock = new Mock<IFileLoggerProvider>();
            providerMock.SetupGet(p => p.LogFolder).Returns("logs");
            providerMock.SetupGet(p => p.EventOptionsDict).Returns( new Dictionary<int, EventOptions>()
            {
                // NameExtension shall be ignored if NameExtensionFromEventName == true
                {1, new EventOptions{ Id=1, NameExtensionFromEventName = true, NameExtension = "Ext"}} 
            });
            var logger = new FileLogger(providerMock.Object, "log1");

            logger.Log<object>(LogLevel.Critical, new EventId(1, "event"), new object(), null, (o,e) => "" );
            logger.Log<object>(LogLevel.Critical, new EventId(2, "event"), new object(), null, (o,e) => "" );

            var expectedFilePath1 = $"logs/log1_event_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            var expectedFilePath2 = $"logs/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";

            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath1)), Times.Once);
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath2)), Times.Once);
        }

        [Fact]
        public void SubFolderTest(){

            var providerMock = new Mock<IFileLoggerProvider>();
            providerMock.SetupGet(p => p.LogFolder).Returns("logs");
            providerMock.SetupGet(p => p.EventOptionsDict).Returns( new Dictionary<int, EventOptions>()
            {
                {1, new EventOptions{ Id=1, SubFolder = "sub"}}
            });
            var logger = new FileLogger(providerMock.Object, "log1");

            logger.Log<object>(LogLevel.Critical, 1, new object(), null, (o,e) => "" );
            logger.Log<object>(LogLevel.Critical, 2, new object(), null, (o,e) => "" );

            var expectedFilePath1 = $"logs/sub/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            var expectedFilePath2 = $"logs/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";

            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath1)), Times.Once);
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath2)), Times.Once);
        }

        [Fact]
        public void SubFolderFromEventTest(){

            var providerMock = new Mock<IFileLoggerProvider>();
            providerMock.SetupGet(p => p.LogFolder).Returns("logs");
            providerMock.SetupGet(p => p.EventOptionsDict).Returns( new Dictionary<int, EventOptions>()
            {
                // SubFolder shall be ignored if SubFolderFromEventName == true
                {1, new EventOptions{ Id=1, SubFolderFromEventName = true, SubFolder = "notUsed"}},
                {3, new EventOptions{ Id=3, SubFolderFromEventName = true, SubFolder = "notUsed"}}
            });
            var logger = new FileLogger(providerMock.Object, "log1");

            logger.Log<object>(LogLevel.Critical, new EventId(1, "event/sub"), new object(), null, (o,e) => "" );
            logger.Log<object>(LogLevel.Critical, 2, new object(), null, (o,e) => "" );
            logger.Log<object>(LogLevel.Critical, 3, new object(), null, (o,e) => "" );

            var expectedFilePath1 = $"logs/event/sub/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            var expectedFilePath2 = $"logs/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            // event 3 does not provide an event name and will therfore only use the event's id as sub folder
            var expectedFilePath3 = $"logs/3/log1_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath1)), Times.Once);
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath2)), Times.Once);
            providerMock.Verify(p => p.AddToLogQueue(It.Is<LogMessage>(lm => lm.FullFilePath == expectedFilePath3)), Times.Once);
        }

        
    }
}