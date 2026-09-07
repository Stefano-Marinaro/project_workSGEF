using GoCare.Application.Data;
using GoCare.Application.Data.Auth.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoCare.Application.Tests;

public sealed class AuthModelSnapshotTests
{
    [Fact]
    public void SnapshotMatchesCurrentAuthModel()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var context = new AuthDbContext(options);
        var differ = context.GetService<IMigrationsModelDiffer>();
        var snapshot = new AuthDbContextModelSnapshot();
        var initializer = context.GetService<IModelRuntimeInitializer>();
        var snapshotModel = initializer.Initialize(snapshot.Model, designTime: true);
        var designModel = context.GetService<IDesignTimeModel>().Model;
        var differences = differ.GetDifferences(snapshotModel.GetRelationalModel(), designModel.GetRelationalModel());

        Assert.True(differences.Count == 0,
            string.Join(Environment.NewLine, differences.Select(difference => difference.ToString())));
    }
}
