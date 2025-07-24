using FluentAssertions;
using NUnit.Framework;
using System;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Events;

namespace Todo_App.Domain.UnitTests.Entities;

public class TodoItemTests
{
    [Test]
    public void Setting_Done_To_True_Should_Add_TodoItemCompletedEvent()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Title = "Test item",
            Done = false
        };

        // Act
        todoItem.Done = true;

        // Assert
        var domainEvent = todoItem.DomainEvents.OfType<TodoItemCompletedEvent>().FirstOrDefault();
        Assert.IsNotNull(domainEvent, "Expected a TodoItemCompletedEvent to be added.");
        Assert.AreSame(todoItem, domainEvent!.Item, "Event should reference the correct TodoItem.");
    }

    [Test]
    public void Setting_Done_To_True_When_Already_True_Should_Not_Add_DuplicateEvent()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Title = "Test item",
            Done = true
        };

        // Clear existing domain events
        todoItem.ClearDomainEvents();

        // Act
        todoItem.Done = true;

        // Assert
        var domainEvent = todoItem.DomainEvents.OfType<TodoItemCompletedEvent>().FirstOrDefault();
        Assert.IsNull(domainEvent, "Should not add TodoItemCompletedEvent when already Done.");
    }

    [Test]
    public void Setting_Done_To_False_Should_Not_Add_Event()
    {
        // Arrange
        var todoItem = new TodoItem
        {
            Title = "Test item",
            Done = true
        };

        // Clear existing domain events
        todoItem.ClearDomainEvents();

        // Act
        todoItem.Done = false;

        // Assert
        Assert.IsEmpty(todoItem.DomainEvents, "Should not add event when setting Done to false.");
    }
}

