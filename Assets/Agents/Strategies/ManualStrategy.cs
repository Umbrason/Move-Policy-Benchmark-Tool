public class ManualStrategy : ITeamStrategy
{
    private readonly Team team;
    private readonly CopsNRobberGame game;

    public ManualStrategy(Team team, CopsNRobberGame game)
    {
        this.team = team;
        this.game = game;
    }

    public void Init() { }

    public void Tick()
    {
        for (int i = 0; i < game.teamSpeed[team]; i++)
            foreach (var agent in team.Agents) ManualModeInputHandler.RequestMoveSelection(agent);
    }
}
