public class ManualStrategy : ITeamStrategy
{
    private readonly Team team;

    public ManualStrategy(Team team)
    {
        this.team = team;
    }

    public void Init() { }

    public void Tick()
    {
        foreach (var agent in team.agents) ManualModeInputHandler.RequestMoveSelection(agent);
    }
}
