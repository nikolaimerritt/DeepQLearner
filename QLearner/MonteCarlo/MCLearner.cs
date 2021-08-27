using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
namespace DeepLearning
{
    public class MCLearner<TGame, TMove>
        where TGame : IEnvironment<TMove>
    {
        private MCNode<TGame, TMove> _root;
        private TGame _game;
        private QLearner<TGame, TMove> _qlearner;

        public MCLearner(TGame game, QLearner<TGame, TMove> qlearner)
        {
            _game = game;
            _qlearner = qlearner;
            _root = new(parent: null, move: default, rolloutValue: 0);
            foreach (TMove move in _game.AllPossibleMoves.Where(_game.IsValidMove))
            {
                _game.MakeMove(move);
            }
        }

        private void InitialiseChildren(TGame game, MCNode<TGame, TMove> parent)
        {
            game.Reset();
            foreach (TMove move in parent.MoveSequence)
                game.MakeMove(move);
        }

        public double RolloutValue(TGame game)
        {
            int turns = 0;
            while (!game.IsTerminal())
            {
                _qlearner.MakeMove(game);
                turns++;
            }
            if (game.HasWon())
                return 10.0 / turns;
            else return -10.0 / turns;
        }
    }
}
*/