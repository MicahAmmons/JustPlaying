using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlayingAround.Entities.Monster.CombatMonsters;
using PlayingAround.Entities.Monster.PlayMonsters;
using PlayingAround.Entities.Player;
using System.Collections.Generic;

public class CombatManager
{
    private Player player;
    private List<CombatMonster> enemyMonsters;
    private List<CombatMonster> friendlyMonsters;

    private Queue<ICombatEntity> turnQueue; // Both friendly and enemy entities
    private ICombatEntity currentTurnEntity;

    public CombatManager(Player player, PlayMonsters playMonster)
    {
        this.player = player;
        this.enemyMonsters = playMonster.Monsters;

        InitializeTurnQueue();
    }

    private void InitializeTurnQueue()
    {
        var allEntities = new List<ICombatEntity>();
        allEntities.AddRange(friendlyMonsters);
        allEntities.AddRange(enemyMonsters);

        // Order by speed (or other logic)
        turnQueue = new Queue<ICombatEntity>(allEntities.OrderByDescending(e => e.Speed));
    }

    public void Update(GameTime gameTime)
    {
        if (currentTurnEntity == null && turnQueue.Any())
        {
            currentTurnEntity = turnQueue.Dequeue();
            currentTurnEntity.StartTurn(this);
        }

        currentTurnEntity?.Update(gameTime);
    }

    public void EndTurn()
    {
        if (currentTurnEntity != null)
        {
            turnQueue.Enqueue(currentTurnEntity);
            currentTurnEntity = null;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var mon in friendlyMonsters)
            mon.Draw(spriteBatch);

        foreach (var mon in enemyMonsters)
            mon.Draw(spriteBatch);

        player.DrawUI(spriteBatch); // if needed
    }

    public void RemoveEntity(ICombatEntity entity)
    {
        // Remove from active lists and turn queue
        if (entity is CombatMonster mon)
        {
            enemyMonsters.Remove(mon);
            friendlyMonsters.Remove(mon);
        }

        var tempQueue = new Queue<ICombatEntity>(turnQueue.Where(e => e != entity));
        turnQueue = tempQueue;
    }
}
