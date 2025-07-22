using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItem;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.UnitTests.TodoItems.Commands;

public class UpdateTodoItemCommandTests
{
    [Test]
    public async Task ShouldUpdateTodoItemColour()
    {
        // Arrange
        var item = new TodoItem { Id = 1, Title = "Old", Colour = "#000000" };

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(x => x.TodoItems.FindAsync(new object[] { 1 }, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(item);
        mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

        var handler = new UpdateTodoItemCommandHandler(mockContext.Object);

        var command = new UpdateTodoItemCommand
        {
            Id = 1,
            Title = "Updated",
            Done = true,
            Colour = "#ABC123"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        item.Colour.Should().Be("#ABC123");
        mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
