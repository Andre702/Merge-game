using UnityEngine;

public class TileBlock : MonoBehaviour
{
    public SpriteRenderer sprite;

    public void Highlight(bool highlight)
    {
        if (highlight == true)
        {
            sprite.color = Color.gray;
        }
        else
        {
            sprite.color = Color.white;
        }
    }

}
