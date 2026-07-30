using Microsoft.AspNetCore.Mvc;
using RoleFit.Api.Contracts;
using RoleFit.Api.Controllers;
using Xunit;

namespace RoleFit.Api.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        var controller = new HealthController();

        var result = controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var health = Assert.IsType<HealthResponse>(okResult.Value);
        Assert.Equal("healthy", health.Status);
    }
}
