using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhotonEvents
{
    public const float TICK_SPEED = 1f / 30f;

    public const byte MOVE_EVENT_CODE = 0; //startX(int), startY(int), endX(int), endY(int), neststartX(int), neststartY(int)... etc
    public const byte MESSAGE_EVENT_CODE = 1; //message(string)
    public const byte TIMER_UPDATE_EVENT_CODE = 2; //whiteTime(float), blackTime(float)
    public const byte PAWN_PROMOTION_EVENT_CODE = 3; //xPos(int), yPos(int), pieceType(int)
    public const byte UPDATED_SETTINGS_EVENT_CODE = 4; //gameTime(float), hostWhite(bool)
    public const byte RESTART_GAME_EVENT_CODE = 5; //
}
