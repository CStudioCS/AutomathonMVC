from gym import Gym
from datatypes import AIAction, Vector2


env = Gym()

num_episodes = 1

for i_episode in range(num_episodes):
    # Initialize the environment and get its state
    state = env.reset()

    while not state.Done:
        #this is where you do all the AI stuff you want to get an AIAction object using state
        action = AIAction()
        action.MovingDirection = Vector2(700, 500) 
        
        next_state = env.step(action)
        reward = 10 # determine the best reward function yourself !!

        # Store the transition in memory
        # memory.push(state, action, next_state, reward)

        state = next_state

        # Perform one step of the optimization (on the policy network)
        # optimize_model()

print('Complete')