using UnityEngine;

[ExecuteInEditMode]
public class RandomizeSeed : MonoBehaviour {
    [SerializeField] private Material baseMaterial;
    private static readonly int OffsetProperty = Shader.PropertyToID("_Offset");
    [SerializeField] private Vector2 currentOffset;

    [SerializeField] private MaterialPropertyBlock propertyBlock;
    [SerializeField] private Renderer rendererComponent;

    void Awake() {
        propertyBlock = new MaterialPropertyBlock();
        rendererComponent = GetComponent<Renderer>();
    }

    void Start() {
        ApplyRandomOffset();
    }

    void OnEnable() {
        ApplyRandomOffset();
    }

    private void ApplyRandomOffset() {
        if (rendererComponent != null && baseMaterial != null && propertyBlock != null) {
            currentOffset = new Vector2(Random.Range(0.0f, 1000.0f), Random.Range(0.0f, 1000.0f));
            rendererComponent.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(OffsetProperty, currentOffset);
            rendererComponent.SetPropertyBlock(propertyBlock);
        }
    }

    // Optional: Method to manually update the offset
    public void UpdateOffset(Vector2 newOffset) {
        currentOffset = newOffset;
        rendererComponent.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(OffsetProperty, currentOffset);
        rendererComponent.SetPropertyBlock(propertyBlock);
    }
}