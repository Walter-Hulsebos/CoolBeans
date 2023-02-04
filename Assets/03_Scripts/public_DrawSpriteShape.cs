using UnityEngine;
using UnityEngine.U2D;

public class public_DrawSpriteShape : MonoBehaviour{
//Description:  Allows the user to draw a shape using a mouse or touchscreen. When shape is completed, ensure it is 'closed off'
//                  and the shape's verticies are passed into a seperate function to make the shape either a functioning G_Zone
//                  or terrain object.

    private Camera mainCam;                                 //The main camera's camera component
    private GameObject newObject;                           //The GameObjrct to be created for each shape
    private LineRenderer lineRenderer;                      //The LineRenderer component used to preview the shape before proper creation
    private SpriteShapeController ssController;             //The SpriteShapeController component used to construct the final shape (pairs with SpriteShapeRenderer)
    private SpriteShapeRenderer ssRenderer;                 //The SpriteShapeRenderer component used to visually render the final shape (pairs with SpriteShapeController)
    private PolygonCollider2D polyCol;                      //The PolygonCollider2D partnered with the shape
    private Vector2 lastPoint = Vector2.zero;               //The worldspace positional vector of the last placed vertex of our LineRenderer

    [Header("Shape Properties")]
    public float minimumVertexDistance = 1f;                //The minimum worldspace distance between verticies in the shape
    

    [Header("LineRenderer Materials")]
    public Material lineMat;                                //The material to use for the LineRenderer depicting this shape                  

    [Header("SpriteShape Settings")]
    public SpriteShape ssProfile;                           //The SpriteShape profile used by our shape

    private void Start()
    {
        mainCam = Camera.main;
        //this.enabled = false;
    }

    private void SetUpNewObject()
    {
        //Creating the actual shape object (Later modified into terrain or G_Zone or whatever)
        // System.Type[] x = new System.Type[4];
        //     x[0] = typeof(SpriteShapeRenderer);
        //     x[1] = typeof(SpriteShapeController);
        //     x[2] = typeof(LineRenderer);
        //     x[3] = typeof(PolygonCollider2D);

        newObject = new GameObject("shape", typeof(SpriteShapeRenderer), typeof(SpriteShapeController), typeof(LineRenderer), typeof(PolygonCollider2D));
        
        //Getting referances to the object's components
        ssController = newObject.GetComponent<SpriteShapeController>();
        ssRenderer   = newObject.GetComponent<SpriteShapeRenderer>();
        lineRenderer = newObject.GetComponent<LineRenderer>();
        polyCol      = newObject.GetComponent<PolygonCollider2D>();
        
        //Setting some spriteshapecontroller properties
        ssController.spriteShape  = ssProfile;
        ssController.splineDetail = 16;
        ssController.spline.isOpenEnded = true;
        
        //Disabling the SpriteShape renderer (for now)
        ssRenderer.enabled = false;
        
        //Setting some linerenderer properties
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.loop = true;
        lineRenderer.material = lineMat;
        
        //Setting some properties relating to the PolygonCollider2D
        ssController.autoUpdateCollider = true;
        ssController.colliderDetail = 16;
        
        //Setting the object's position to the mouse's current position (as this is on first click/tap)
        newObject.transform.position = (Vector2) mainCam.ScreenToWorldPoint(Input.mousePosition);
        //Setting a clean slate for the linerenderer
        lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        //Setting the shapeObjec's position to the first point in the line, will run once
        if(Input.GetMouseButtonDown(0))
        {
            SetUpNewObject();
        }

        //If holding the mouse button OR touchscreen
        if(Input.GetMouseButton(0))
        {
            //The current mouse/touch position in screen-space
            Vector2 currentMousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            //Comparing the old and new point's distance againt the pre-defined minimum value
            if(Vector2.Distance(lastPoint, currentMousePosition) >= minimumVertexDistance)
            {
                //Drawing lines between the last and current vertex of the shape
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount-1, currentMousePosition);
                //Setting the lastPoint value to the new latest value
                lastPoint = currentMousePosition;
            }
        }

        //When the shape drawing is concluded
        if(Input.GetMouseButtonUp(0))
        {
            CommitToSpriteShape();
        }
    }


///////////////////////////////////////////////////////////////////////////////////////////////////////////
//For when the drawing is complete and the shape is to be turned into SpriteShape

    public void CommitToSpriteShape()
    {
        //Converting the LineRenderer into a SpriteShape
        LineToSpriteShape();
        //Disabling this script
        this.enabled = false;
    }

    private void LineToSpriteShape(){
        //Extracting an array of position vectors from the LineRenderer's verticies
        int numOfPoints = lineRenderer.positionCount;
        Vector3[] linePosses = new Vector3[numOfPoints]; 
        lineRenderer.GetPositions(linePosses);
        
        //Editing the spriteshape's spline properties
        for(int i = 0; i<numOfPoints; i++)
        {
            ssController.spline.InsertPointAt(i, linePosses[i] - newObject.transform.position);
        }

        //Making changes to some components
        Destroy(lineRenderer);
        ssRenderer.enabled = true;
    }
}