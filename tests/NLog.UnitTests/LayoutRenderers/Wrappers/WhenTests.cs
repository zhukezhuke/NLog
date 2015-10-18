// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.UnitTests.LayoutRenderers.Wrappers
{
    using NLog;
    using NLog.Layouts;
    using Xunit;

    public class WhenTests : NLogTestBase
    {
        [Fact]
        public void PositiveWhenTest()
        {
            SimpleLayout l = @"${message:when=logger=='logger'}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("message", l.Render(le));
        }

        [Fact]
        public void NegativeWhenTest()
        {
            SimpleLayout l = @"${message:when=logger=='logger'}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger2", "message");
            Assert.Equal("", l.Render(le));
        }

        [Fact]
        public void ComplexWhenTest()
        {
            // condition is pretty complex here and includes nested layout renderers
            // we are testing here that layout parsers property invokes Condition parser to consume the right number of characters
            SimpleLayout l = @"${message:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger':padding=-10:padCharacter=Y}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("messageYYY", l.Render(le));
        }

        [Fact]
        public void ComplexWhenTest2()
        {
            // condition is pretty complex here and includes nested layout renderers
            // we are testing here that layout parsers property invokes Condition parser to consume the right number of characters
            SimpleLayout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger'}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("messageYYY", l.Render(le));
        }

        [Fact]
        public void ComplexWhenWithColonTest_with_workaround()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test${literal:text=\:} Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test: Hello", l.Render(le));
        }

        [Fact]
        public void ComplexWhenWithColonTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test\: Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test: Hello", l.Render(le));
        }

        [Fact]
        public void ComplexWhenWithSlashTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test\\Hello}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test\\Hello", l.Render(le));
        }

        [Fact]
        public void ComplexWhenWithBracketsTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:Inner=Test{Hello\}}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Test{Hello}", l.Render(le));
        }


        [Fact]
        public void ComplexWhenWithHashTest()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=Log_{#\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("Log_{#}.log", l.Render(le));
        }
        [Fact]
        public void ComplexWhenWithHashTest_and_layoutrender()
        {
            SimpleLayout l = @"${when:when=1 == 1:inner=${counter}/Log_{#\}.log}";

            var le = LogEventInfo.Create(LogLevel.Info, "logger", "message");
            Assert.Equal("1/Log_{#}.log", l.Render(le));
        }
    }
}