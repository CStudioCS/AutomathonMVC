from bridge.gym import Gym
from bridge.datatypes import *
import copy
import example_ai # this is where your ai will actually be implemented

def train():
    env = Gym()
    num_episodes = 50

    print("Begin Training")

    for i_episode in range(num_episodes):
        # Initialize the environment and get its state
        # Note : The environment given by the game is not normalized, neither is it preprocessed
        # If you want a tank lidar etc, you'll have to build it yourself
        state = env.reset()
        print(f"Episode {i_episode}")

        while not state.Done and state.SelfTank != None:
            
            action = example_ai.decide_action(state)
        
            state_from_enemy_pov: GameState = copy.deepcopy(state)
            state_from_enemy_pov.SelfTank = state.EnemyTank
            state_from_enemy_pov.EnemyTank = state.SelfTank
            enemy_action = example_ai.decide_action(state) # in training, you decide the action of both players

            #this can raise a timeout error if your game's headless version isn't running before you start the project (or if the game takes more than 500ms to respond)
            next_state = env.step(action, enemy_action) 

            reward = 10 # determine the best reward function yourself !!

            # Store the transition in memory
            # memory.push(state, action, next_state, reward)
            # Perform one step of the optimization (on the policy network)
            # optimize_model()

            state = next_state

    env.end_training()

    print('Training Complete')

train()