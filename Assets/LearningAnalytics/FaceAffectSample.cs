using System;
using UnityEngine;

/// <summary>
/// Uma amostra de expressão facial: os 52 blendshapes do MediaPipe FaceLandmarker
/// (padrão ARKit) e alguns índices derivados.
///
/// IMPORTANTE: os índices derivados são HEURÍSTICOS e NÃO são medidas validadas de
/// emoção. Use-os para exploração; para análise, prefira os blendshapes brutos
/// triangulados com autorrelato e logs (veja README.md).
/// </summary>
public class FaceAffectSample
{
    // Ordem canônica dos 52 blendshapes devolvidos pelo modelo face_landmarker_v2_with_blendshapes.
    public static readonly string[] BlendshapeNames =
    {
        "_neutral", "browDownLeft", "browDownRight", "browInnerUp", "browOuterUpLeft", "browOuterUpRight",
        "cheekPuff", "cheekSquintLeft", "cheekSquintRight", "eyeBlinkLeft", "eyeBlinkRight",
        "eyeLookDownLeft", "eyeLookDownRight", "eyeLookInLeft", "eyeLookInRight", "eyeLookOutLeft",
        "eyeLookOutRight", "eyeLookUpLeft", "eyeLookUpRight", "eyeSquintLeft", "eyeSquintRight",
        "eyeWideLeft", "eyeWideRight", "jawForward", "jawLeft", "jawOpen", "jawRight", "mouthClose",
        "mouthDimpleLeft", "mouthDimpleRight", "mouthFrownLeft", "mouthFrownRight", "mouthFunnel",
        "mouthLeft", "mouthLowerDownLeft", "mouthLowerDownRight", "mouthPressLeft", "mouthPressRight",
        "mouthPucker", "mouthRight", "mouthRollLower", "mouthRollUpper", "mouthShrugLower",
        "mouthShrugUpper", "mouthSmileLeft", "mouthSmileRight", "mouthStretchLeft", "mouthStretchRight",
        "mouthUpperUpLeft", "mouthUpperUpRight", "noseSneerLeft", "noseSneerRight",
    };

    public static readonly string[] DerivedNames =
    {
        "smile", "frown", "brow_down", "brow_inner_up", "brow_outer_up", "eye_wide", "eye_squint",
        "eye_blink", "jaw_open", "mouth_press", "nose_sneer", "gaze_away",
    };

    public DateTime utc;
    public double sessionMs;
    public string scene;
    public bool facePresent;

    /// <summary>Valores na mesma ordem de <see cref="BlendshapeNames"/>; NaN quando não há rosto.</summary>
    public readonly float[] blendshapes = new float[BlendshapeNames.Length];

    /// <summary>Valores na mesma ordem de <see cref="DerivedNames"/>.</summary>
    public readonly float[] derived = new float[DerivedNames.Length];

    /// <summary>Orientação aproximada da cabeça em graus (yaw = esquerda/direita, pitch = cima/baixo).</summary>
    public float headYaw = float.NaN, headPitch = float.NaN, headRoll = float.NaN;

    public float this[string name]
    {
        get
        {
            int i = Array.IndexOf(BlendshapeNames, name);
            return i >= 0 ? blendshapes[i] : float.NaN;
        }
    }

    public void ClearFace()
    {
        facePresent = false;
        for (int i = 0; i < blendshapes.Length; i++) blendshapes[i] = float.NaN;
        for (int i = 0; i < derived.Length; i++) derived[i] = float.NaN;
        headYaw = headPitch = headRoll = float.NaN;
    }

    /// <summary>Calcula os índices derivados a partir dos blendshapes já preenchidos.</summary>
    public void ComputeDerived()
    {
        float Avg(string a, string b) => (this[a] + this[b]) * 0.5f;

        derived[0] = Avg("mouthSmileLeft", "mouthSmileRight");      // sorriso
        derived[1] = Avg("mouthFrownLeft", "mouthFrownRight");      // canto da boca para baixo
        derived[2] = Avg("browDownLeft", "browDownRight");          // sobrancelha franzida (esforço/confusão)
        derived[3] = this["browInnerUp"];                           // sobrancelha interna erguida (preocupação/surpresa)
        derived[4] = Avg("browOuterUpLeft", "browOuterUpRight");    // sobrancelha externa erguida (surpresa)
        derived[5] = Avg("eyeWideLeft", "eyeWideRight");            // olhos arregalados
        derived[6] = Avg("eyeSquintLeft", "eyeSquintRight");        // olhos semicerrados
        derived[7] = Avg("eyeBlinkLeft", "eyeBlinkRight");          // olhos fechados
        derived[8] = this["jawOpen"];                               // boca aberta
        derived[9] = Avg("mouthPressLeft", "mouthPressRight");      // lábios pressionados (tensão)
        derived[10] = Avg("noseSneerLeft", "noseSneerRight");       // nariz franzido (nojo/desconforto)

        // Olhar desviado: maior desvio de olhar entre as direções.
        float lookDown = Avg("eyeLookDownLeft", "eyeLookDownRight");
        float lookUp = Avg("eyeLookUpLeft", "eyeLookUpRight");
        float lookSide = Mathf.Max(Avg("eyeLookOutLeft", "eyeLookInRight"), Avg("eyeLookInLeft", "eyeLookOutRight"));
        derived[11] = Mathf.Max(lookDown, Mathf.Max(lookUp, lookSide));
    }

    public void SetHeadPose(Matrix4x4 m)
    {
        // A matriz de transformação facial do MediaPipe usa um sistema destro;
        // os ângulos abaixo são aproximados, suficientes para "olhando para a tela ou não".
        Vector3 forward = m.GetColumn(2);
        Vector3 up = m.GetColumn(1);
        if (forward.sqrMagnitude < 1e-6f || up.sqrMagnitude < 1e-6f) return;
        forward.Normalize();
        up.Normalize();
        headYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        headPitch = Mathf.Asin(Mathf.Clamp(forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        headRoll = Mathf.Atan2(up.x, up.y) * Mathf.Rad2Deg;
    }
}
