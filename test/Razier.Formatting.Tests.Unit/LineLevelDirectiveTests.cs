using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class LineLevelDirectiveTests
{
    [Fact]
    public void Format_ShouldAppendSemicolon_WhenLineDoesNotEndWithSemicolon()
    {
        // Arrange.
        var input = "\t@using System";

        // Act.
        var formatted = Formatter.Format(input);

        // Assert.
        formatted.Should().Be("@using System;\r\n");
    }

    [Fact]
    public void Format_ShouldNotAppendSemicolon_WhenLineEndsWithSemicolon()
    {
        // Arrange.
        var input = "\t@using System;";

        // Act.
        var formatted = Formatter.Format(input);

        // Assert.
        formatted.Should().Be("@using System;\r\n");
    }

    [Fact]
    public void Format_ShouldNotIndentLineLevelDirective_WhenCalled()
    {
        // Arrange.
        var input = "\t@using System;";

        // Act.
        var formatted = Formatter.Format(input);

        // Assert.
        formatted.Should().Be("@using System;\r\n");
    }
}
