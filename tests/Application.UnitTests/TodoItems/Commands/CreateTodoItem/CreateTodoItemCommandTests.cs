using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Todo_App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Todo_App.Application.UnitTests.TodoItems.Commands;

public class CreateTodoItemCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateTodoItemCommandHandler _handler;

    public CreateTodoItemCommandTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.TodoItems.Add(It.IsAny<TodoItem>()));
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(1);

        _handler = new CreateTodoItemCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task ShouldCreateTodoItem_WithColour()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            Title = "Test Todo",
            Colour = "#FFEE00",
            ListId = 1
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.TodoItems.Add(It.Is<TodoItem>(item => item.Colour == "#FFEE00")), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
