#include <raylib.h> // https://www.raylib.com
#include <stdlib.h>
#include <stdio.h>
#include <time.h>

#define GLSL_VERSION 330


int main(void) {
    const int screen_width = 800;
    const int screen_height = 800;

    InitWindow(screen_width,screen_height, "kunutza_destroyer2");


    // RLAPI void SetShaderValueTexture(Shader shader, int locIndex, Texture2D texture); // Set shader uniform value for texture (sampler2d)
    Image planetimBlank = GenImageColor(100, 100, BLANK);
    Texture2D planet_texture = LoadTextureFromImage(planetimBlank);  // Load blank texture to fill on shader
    UnloadImage(planetimBlank);

    Shader planet_shader = LoadShader(0, TextFormat("shaders/planet.fs", GLSL_VERSION));
    
    // set the uniform for (vec2 circleCenter) in circle.fs
    float circleCenter[2] = { (float)planetimBlank.width/2,  screen_height - (float)planetimBlank.height/2 };
    int circleCenterLoc = GetShaderLocation(planet_shader, "circleCenter");
    SetShaderValue(planet_shader, circleCenterLoc, circleCenter, SHADER_UNIFORM_VEC2);
    // set the uniform for (float circleRadius) in circle.fs
    float circleRadius = 50.0;
    int circleRadiusLoc = GetShaderLocation(planet_shader, "circleRadius");
    SetShaderValue(planet_shader, circleRadiusLoc, &circleRadius, SHADER_UNIFORM_FLOAT);
    // set the uniform for (vec2 u_resolution) in circle.fs
    float iResolution[2] = { screen_width, screen_height};
    int iResolutionLoc = GetShaderLocation(planet_shader, "iResolution");
    SetShaderValue(planet_shader, iResolutionLoc, iResolution, SHADER_UNIFORM_VEC2);
    // set the uniform for θχ (float rotation) in circle.fs
    float rotation = 0.0;
    int rotationLoc = GetShaderLocation(planet_shader, "rotation");
    SetShaderValue(planet_shader, rotationLoc, &rotation, SHADER_UNIFORM_FLOAT);
    // set the uniform for (float time) in circle.fs
    srand(time(NULL));
    float ctime;
    float time_multiplier = 1.0;
    ctime = rand() % 10000;
    int timeLoc = GetShaderLocation(planet_shader, "iTime");
    SetShaderValue(planet_shader, timeLoc, &ctime, SHADER_UNIFORM_FLOAT);
    // set the uniform for (float seed) in circle.fs
    int seed = rand() % 100000 ;
    int seedLoc = GetShaderLocation(planet_shader, "seed");
    SetShaderValue(planet_shader, seedLoc, &seed, SHADER_UNIFORM_INT);


    // The skymap
    Image skymapimBlank = GenImageColor(iResolution[0], iResolution[1], BLANK);
    Texture2D skymap_texture = LoadTextureFromImage(skymapimBlank);  // Load blank texture to fill on shader
    UnloadImage(skymapimBlank);

    Shader skymap_shader = LoadShader(0, TextFormat("shaders/skymap.fs", GLSL_VERSION));
    
    // set the uniform for (vec2 iResolution) in skymap.fs
    int u_resolutionLoc = GetShaderLocation(skymap_shader, "iResolution");
    SetShaderValue(skymap_shader, u_resolutionLoc, iResolution, SHADER_UNIFORM_VEC2);
    // set the uniform for (float iTime) in skymap.fs 
    int itimeLoc = GetShaderLocation(skymap_shader, "iTime");
    SetShaderValue(skymap_shader, itimeLoc, &ctime, SHADER_UNIFORM_FLOAT);

    // Load texture for rendering (framebuffer), after that I will update it
    RenderTexture2D target = LoadRenderTexture(iResolution[0], iResolution[1]);

    float delta;
    SetTargetFPS(60);

    while (!WindowShouldClose()) {
        delta = GetFrameTime();
        if (ctime <= 0) {
            time_multiplier = -time_multiplier;
        } else if (ctime >= 100000) {
            time_multiplier = -time_multiplier;
        }
        ctime += time_multiplier * delta;
        SetShaderValue(planet_shader, timeLoc, &ctime, SHADER_UNIFORM_FLOAT);
        SetShaderValue(skymap_shader, itimeLoc, &ctime, SHADER_UNIFORM_FLOAT);

        // make the rotation number never gets too big
        if (rotation >= 32*PI ) {
            rotation -= 32*PI ;
        }
        rotation += 1.0 * delta;
        SetShaderValue(planet_shader, rotationLoc, &rotation, SHADER_UNIFORM_FLOAT);

        // framebuffer
        BeginTextureMode(target);

        BeginShaderMode(skymap_shader);
        DrawTexture(skymap_texture, 0, 0, WHITE);
        EndShaderMode();
        
        BeginShaderMode(planet_shader);
        DrawTexture(planet_texture, 0, 0, WHITE);
        EndShaderMode();

        EndTextureMode();

        BeginDrawing();
        
        ClearBackground(BLACK);

        DrawTextureRec(target.texture, (Rectangle){ 0, 0, (float)target.texture.width, (float)-target.texture.height }, (Vector2){ 0, 0 }, WHITE);

        EndDrawing();
    }

    UnloadRenderTexture(target);
    UnloadShader(planet_shader);
    UnloadShader(skymap_shader);

    CloseWindow();

    exit(EXIT_SUCCESS);
}

