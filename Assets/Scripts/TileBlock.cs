using UnityEngine;

public class TileBlock : MonoBehaviour
{
    public SpriteRenderer sprite;

    public void Highlight(bool highlight)
    {
        if (highlight == true)
        {
            sprite.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        }
        else
        {
            sprite.color = Color.white;
        }
    }

}
