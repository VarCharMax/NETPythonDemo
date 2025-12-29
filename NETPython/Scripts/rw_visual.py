"""Visualize a random walk using matplotlib."""

import matplotlib.pyplot as plt
from random_walk import RandomWalk


# Make a random walk.
def create_plot() -> None:
    """_summary_"""
    rw = RandomWalk()
    rw.fill_walk()

    # Plot the points in the walk.
    plt.style.use("classic")
    fig, ax = plt.subplots(figsize=(15, 9))
    point_numbers = range(rw.num_points)
    ax.scatter(rw.x_values, rw.y_values, c=point_numbers, cmap=plt.cm.Blues, edgecolors="none", s=1)  # type: ignore
    ax.set_aspect("equal")

    ax.scatter(0, 0, c="green", edgecolors="none", s=100)
    ax.scatter(rw.x_values[-1], rw.y_values[-1], c="red", edgecolors="none", s=100)

    ax.get_xaxis().set_visible(False)
    ax.get_yaxis().set_visible(False)

    plt.savefig("random_walk.png")


if __name__ == "__main__":
    create_plot()
