# War3ReplayAnalyzer
A program that does sentiment analysis on the chat of a Warcraft III replay and predicts the winner of the game.

# Results

Total Replays: 2000

Uses Naive Bayes (Bernoulli) to do sentiment analysis of replays


Without Bigrams:

| Training | Test | Accuracy |
|----------|------|----------|
| 200      | 200  | 83.78%   |
| 500      | 500  | 84.50%   |
| 1000     | 1000 | 84.41%   |
| 1500     | 500  | 82.90%   |


With Bigrams:

| Training | Test | Accuracy |
|----------|------|----------|
| 200      | 200  | 82.20%   |
| 500      | 500  | 83.73%   |
| 1000     | 1000 | 83.68%   |
| 1500     | 500  | 81.34%   |
