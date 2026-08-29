using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Services;
using FluentAssertions;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class DepthGuardTests
{
    [Theory]
    [InlineData(null, 3)]
    [InlineData(1, 1)]
    [InlineData(6, 6)]
    public void Normalize_AcceptsValidValues(int? value, int expected)
    {
        DepthGuard.Normalize(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(-3)]
    public void Normalize_RejectsOutOfRange(int value)
    {
        var act = () => DepthGuard.Normalize(value);
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    public void NormalizeDependencyDepth_AcceptsValidValues(int value)
    {
        DepthGuard.NormalizeDependencyDepth(value).Should().Be(value);
    }

    [Fact]
    public void NormalizeDependencyDepth_Default_IsFive()
    {
        DepthGuard.NormalizeDependencyDepth(null).Should().Be(5);
    }

    [Fact]
    public void NormalizeDependencyDepth_RejectsLargeValue()
    {
        var act = () => DepthGuard.NormalizeDependencyDepth(42);
        act.Should().Throw<ValidationException>();
    }
}