using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Good
{
    public static class Transform3D
    {
        public static Vector3 SetX(float x, Transform t)
        {
            return new Vector3(x, t.localPosition.y, t.localPosition.z);
        }

        public static Vector3 SetY(float y, Transform t)
        {
            return new Vector3(t.localPosition.x, y, t.localPosition.z);
        }

        public static Vector3 SetZ(float z, Transform t)
        {
            return new Vector3(t.localPosition.x, t.localPosition.y, z);
        }
    }

    public static class MathS
    {
        public static GameObject FindNearTarget(string tag, GameObject myObj)
        {
            List<GameObject> objects = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag));
            if (objects.Count == 0)
            {
                return null;
            }
            if (tag == myObj.tag && objects.Count > 1)
                objects.Remove(myObj);

            GameObject nearTarget = objects[0];
            float nearDistance = Vector3.Distance(myObj.transform.position, nearTarget.transform.position);
            foreach (var obj in objects)
            {
                float distance = Vector3.Distance(myObj.transform.position, obj.transform.position);
                if (distance < nearDistance)
                {
                    nearTarget = obj;
                    nearDistance = distance;
                }
            }
            return nearTarget;
        }

        public static Quaternion LookAngle(Vector3 dir)
        {
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.back);
        }
    }
}
