using FluentAssertions;
using NUnit.Framework;
using System;
using Todo_App.Domain.Entities;

namespace Todo_App.Domain.UnitTests.Entities;

public class TodoItemTests
{
    [Test]
    public void ShouldAllowSettingColour()
    {
        // Arrange
        var todo = new TodoItem();
        var expectedColour = "#FFCC00";

        // Act
        todo.Colour = expectedColour;

        // Assert
        todo.Colour.Should().Be(expectedColour);
    }

    [Test]
    public void ShouldAllowNullColour()
    {
        // Arrange
        var todo = new TodoItem();

        // Act
        todo.Colour = null;

        // Assert
        todo.Colour.Should().BeNull();
    }

    [Test]
    public void ShouldStoreEmptyStringAsColour()
    {
        // Arrange
        var todo = new TodoItem();

        // Act
        todo.Colour = "";

        // Assert
        todo.Colour.Should().Be("");
    }
}
