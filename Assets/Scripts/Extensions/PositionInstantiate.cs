using UnityEngine;

namespace Poligon.Extensions {
    public static class PoligonExtensions {
        public static Object Instantiate(this Object thisObj, Object original, Vector3 position, Quaternion rotation, BulletData bulletData) {
            GameObject bullet = Object.Instantiate(original, position, rotation) as GameObject;
            BulletRaycast scr = bullet.GetComponent<BulletRaycast>();
            scr.Setup(bulletData);
            return bullet;
        }
        public static GameObject Instantiate(this Object thisObj, Object original, Vector3 position, Quaternion rotation, CoverParams coverParams) {
            GameObject coverPosition = Object.Instantiate(original, position, rotation) as GameObject;
            CoverPosition scr = coverPosition.GetComponent<CoverPosition>();
            scr.Setup(coverParams);
            return coverPosition;
        }
    }
}