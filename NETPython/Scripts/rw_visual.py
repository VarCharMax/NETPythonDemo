"""Visualize a random walk using matplotlib."""

import sys
import datetime
import matplotlib.pyplot as plt
from random_walk import RandomWalk

SAVE_FILE = "random_walk"


# Make a random walk.
def create_plot(loopcount: int) -> None:
    """_summary_"""

    save_path = ""

    if sys.platform == "win32":  # Windows
        save_path = "C:\\tmp\\"
    if sys.platform in ["darwin"]:  # Mac OS
        save_path = "/Users/rohanparkes/tmp/"

    plt.style.use("classic")
    cmap_blue = plt.cm.Blues  # type: ignore # pylint: disable=no-member

    i = 0

    while i < loopcount:
        # This must be instantiated inside the loop to get a new walk each time.
        rw = RandomWalk()
        rw.fill_walk()
        # Plot the points in the walk.
        fig, ax = plt.subplots(figsize=(15, 9))
        point_numbers = range(rw.num_points)
        ax.scatter(
            rw.x_values,
            rw.y_values,
            c=point_numbers,
            cmap=cmap_blue,
            edgecolors="none",
            s=1,
        )  # type: ignore

        ax.set_aspect("equal")

        ax.scatter(0, 0, c="green", edgecolors="none", s=100)
        ax.scatter(rw.x_values[-1], rw.y_values[-1], c="red", edgecolors="none", s=100)

        ax.get_xaxis().set_visible(False)
        ax.get_yaxis().set_visible(False)

        current_time = datetime.datetime.now()
        timestamp_str = current_time.strftime("%Y%m%d%H%M%S")

        try:
            if save_path:
                plt.savefig(
                    save_path + SAVE_FILE + f"_{timestamp_str}_{i}.png",
                    bbox_inches="tight",
                )
        except IOError as e:
            print(f"Error saving plot: {e}.")

        plt.close(fig)

        i += 1


if __name__ == "__main__":
    create_plot(5)
