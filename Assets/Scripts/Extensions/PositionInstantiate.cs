using UnityEngine;

namespace Poligon.Extensions {
    public static class PoligonExtensions {
        public static Object Instantiate(this Object thisObj, Object original, Vector3 position, Quaternion rotation, Vector3 targetPosition) {
            GameObject bullet = Object.Instantiate(original, position, rotation) as GameObject;
            BulletRaycast scr = bullet.GetComponent<BulletRaycast>();
            scr.Setup(targetPosition);
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