# Elementary-Chess

This is the complete source for my Unity Chess game, Elementary Chess. You can play the game on my itch.io page:

https://petereldredge.itch.io/elementarychess

All game code and logic was written by me. Photon was used for the networked, room based multiplayer.
The sprites were created by Jake Dixey, and I handled the implementation of all art assets.

The project should be opened in Unity 2019.4 or later. You will notice there is a warning when you start the game.
This is because all of my Photon account information was removed, so it will need to be set up again. This can be done
by creating a new Photon account and using the set up wizard within Unity.

If you do not wish to test the networking, you can still play local matches without issue.

If you just want to view the game code, it is all located in the folder Elementary-Chess\ChessProject\Assets\_Main\Scripts

This project was created for my Software Engineering class and was written over the course of 2-3 weeks. Overall I am very happy with
how the project came out, but there are some issues. You will probably be able to see some hackiness in the game loop, a few too many 
branches, and the checkmate check could use some optimization. This stemmed from the tight timeline and the fact that networked 
multiplayer was added in after the game was already near completion. It was also the first time I had ever worked with networked 
multiplayer, so there was a lot learned over the course of the project. I do still believe that this is a good representation of my 
abilities, and I hope you enjoy looking through the code!

Thanks so much to anyone who takes the time to look through this or any other section of my portfolio, I really appreciate it!
