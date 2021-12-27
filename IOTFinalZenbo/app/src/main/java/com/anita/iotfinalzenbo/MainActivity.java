package com.anita.iotfinalzenbo;

import static android.Manifest.permission.INTERNET;
import static android.Manifest.permission.RECORD_AUDIO;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;

import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.TextView;

import com.asus.robotframework.API.RobotCallback;
import com.microsoft.cognitiveservices.speech.CancellationDetails;
import com.microsoft.cognitiveservices.speech.CancellationReason;
import com.microsoft.cognitiveservices.speech.PropertyId;
import com.microsoft.cognitiveservices.speech.ResultReason;
import com.microsoft.cognitiveservices.speech.SpeechConfig;
import com.microsoft.cognitiveservices.speech.intent.*;

import java.util.concurrent.Future;

public class MainActivity extends AppCompatActivity {

    // Replace below with your LUIS subscription key
    private static String speechSubscriptionKey = "9723e9d2d7f841d085f265ed34563f2e";
    // Replace below with your LUIS service region (e.g., "westus").
    private static String serviceRegion = "westus";
    // Replace below with your LUIS Application ID.
    private static String appId = "05271267-c31e-4ba0-908b-e277b5118087";

    private SpeechConfig speechConfig;
    private IntentRecognizer reco;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Note: we need to request the permissions
        int requestCode = 5; // unique code for the permission request
        ActivityCompat.requestPermissions(MainActivity.this, new String[]{RECORD_AUDIO, INTERNET}, requestCode);

        // Initialize speech synthesizer and its dependencies
        speechConfig = SpeechConfig.fromSubscription(speechSubscriptionKey, serviceRegion);
        speechConfig.setSpeechRecognitionLanguage("zh-TW");
        assert(speechConfig != null);

        reco = new IntentRecognizer(speechConfig);
        assert(reco != null);

        // Creates a language understanding model using the app id, and adds specific intents from your model
        LanguageUnderstandingModel model = LanguageUnderstandingModel.fromAppId(appId);
        reco.addIntent(model, "None");
        reco.addIntent(model, "點餐");
        reco.addIntent(model, "評價");
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        // Release speech synthesizer and its dependencies
        reco.close();
        speechConfig.close();
    }

    public void onSpeechButtonClicked(View v) {
        TextView txt = (TextView) this.findViewById(R.id.hello); // 'hello' is the ID of your text view

        try {
            // Note: this will block the UI thread, so eventually, you want to register for the event
            Future<IntentRecognitionResult> task = reco.recognizeOnceAsync();
            assert(task != null);

            IntentRecognitionResult result = task.get();
            assert(result != null);

            String res = "";

            // Checks result.
            if (result.getReason() == ResultReason.RecognizedIntent) {
                res = res.concat("RECOGNIZED: Text=" + result.getText());
                res = res.concat("    Intent Id: " + result.getIntentId());
                res = res.concat("    Intent Service JSON: " + result.getProperties().getProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult));
            }
            else if (result.getReason() == ResultReason.RecognizedSpeech) {
                res = res.concat("RECOGNIZED: Text=" + result.getText());
                res = res.concat("    Intent not recognized.");
            }
            else if (result.getReason() == ResultReason.NoMatch) {
                res = res.concat("NOMATCH: Speech could not be recognized.");
            }
            else if (result.getReason() == ResultReason.Canceled) {
                CancellationDetails cancellation = CancellationDetails.fromResult(result);
                res = res.concat("CANCELED: Reason=" + cancellation.getReason());

                if (cancellation.getReason() == CancellationReason.Error) {
                    res = res.concat("CANCELED: ErrorCode=" + cancellation.getErrorCode());
                    res = res.concat("CANCELED: ErrorDetails=" + cancellation.getErrorDetails());
                    res = res.concat("CANCELED: Did you update the subscription info?");
                }
            }
            txt.setText(res);

            result.close();
        } catch (Exception ex) {
            Log.e("SpeechSDKDemo", "unexpected " + ex.getMessage());
            assert(false);
        }
    }
}