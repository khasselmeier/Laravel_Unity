<?php

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\AuthController;

//run server on:
    // php -S 127.0.0.1:9011 -t public public/index.php

//Public (no auth required)
Route::get('/health', fn () => response()->json(['ok' => true, 'time' => now()->toISOString()]));

Route::post('/register', [AuthController::class, 'register']);
Route::post('/login',    [AuthController::class, 'login']);

//Protected (requires Authorization: Bearer <token>)
Route::middleware('auth:sanctum')->group(function () {

    //Current logged-in user
    Route::get('/me', function (Request $request) {
        return response()->json($request->user());
    });

    //Logout (revoke current token)
    Route::post('/logout', [AuthController::class, 'logout']);

    //Example protected endpoint (useful to test Unity auth header)
    Route::get('/protected-test', fn () => response()->json([
        'ok' => true,
        'message' => 'You are authenticated!',
    ]));
});
