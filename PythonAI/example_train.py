from gym import Gym
from datatypes import *
import dumbassAI # this is where your ai will actually be implemented

def train():
    env = Gym()
    num_episodes = 1

    print("Begin Training")

    for i_episode in range(num_episodes):
        # Initialize the environment and get its state
        state = env.reset()

        while not state.Done and state.SelfTank != None and state.SelfTank.Position.X < 20000:
            
            action = dumbassAI.decide_action(state)
        
            enemy_action = AIAction() # in training, you decide the action of both players

            next_state = env.step(action, enemy_action) #this can raise a timeout error if your game takes a big 500ms lag spike somehow


            reward = 10 # determine the best reward function yourself !!

            # Note : The environment given by the game is not normalized, neither is it preprocessed
            # If you want a tank lidar etc, you'll have to build it yourself
            # Store the transition in memory
            # memory.push(state, action, next_state, reward)
            # Perform one step of the optimization (on the policy network)
            # optimize_model()

            state = next_state

    env.end_training()

    print('Training Complete')

train()