using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace Tugas_TVG_Hafizh_Aradhana_Harimurti_49249
{
    public class Transformation
    {
        public string TransformName;
        public double amountX, amountY, amountZ, theta, pivotX1, pivotY1, pivotZ1, pivotX2, pivotY2, pivotZ2;

        public Transformation()
        {

        }

        public static double[,] TransformationsToTransformationMatrix(List<Transformation> transformations)
        {
            List<double[,]> transformationMatrices = new List<double[,]>();
            List<Transformation> tempTransformationList = new List<Transformation>();
            double x, y, z, beta, miu;
            foreach(Transformation transformation in transformations)
            {
                switch(transformation.TransformName)
                {
                    case "Scale":
                        {
                            tempTransformationList.Add(new Transformation { TransformName = "Translate", amountX = -transformation.pivotX1, amountY = -transformation.pivotY1, amountZ = -transformation.pivotZ1 });
                            tempTransformationList.Add(new Transformation { TransformName = "Scale", amountX = transformation.amountX, amountY = transformation.amountY, amountZ = transformation.amountZ });
                            tempTransformationList.Add(new Transformation { TransformName = "Translate", amountX = transformation.pivotX1, amountY = transformation.pivotY1, amountZ = transformation.pivotZ1 });
                            break;
                        }
                    case "Rotate":
                        {
                            x = transformation.pivotX2 - transformation.pivotX1;
                            y = transformation.pivotY2 - transformation.pivotY1;
                            z = transformation.pivotZ2 - transformation.pivotZ1;
                            beta = Math.Atan2(x, y);
                            miu = Math.Atan2(Math.Sqrt(Math.Pow(x,2)+Math.Pow(z,2)), y);
                            tempTransformationList.Add(new Transformation { TransformName = "Translate", amountX = -transformation.pivotX1, amountY = -transformation.pivotY1, amountZ = -transformation.pivotZ1 });
                            tempTransformationList.Add(new Transformation { TransformName = "RotateY", theta = -beta });
                            tempTransformationList.Add(new Transformation { TransformName = "RotateX", theta = miu });
                            tempTransformationList.Add(new Transformation { TransformName = "RotateZ", theta = transformation.theta });
                            tempTransformationList.Add(new Transformation { TransformName = "RotateX", theta = -miu });
                            tempTransformationList.Add(new Transformation { TransformName = "RotateY", theta = beta });
                            tempTransformationList.Add(new Transformation { TransformName = "Translate", amountX = transformation.pivotX1, amountY = transformation.pivotY1, amountZ = transformation.pivotZ1 });
                            break;
                        }
                    default:
                        {
                            tempTransformationList.Add(new Transformation
                            {
                                TransformName = transformation.TransformName,
                                amountX = transformation.amountX,
                                amountY = transformation.amountY,
                                amountZ = transformation.amountZ,
                            });
                            break;
                        }
                }    
            }
            double[,] tempMatrix;
            foreach (Transformation tempTransformation in tempTransformationList)
            {
                tempMatrix = IdentityMatrix();
                switch(tempTransformation.TransformName)
                {
                    case "Translate":
                        {
                            tempMatrix[0, 3] = tempTransformation.amountX;
                            tempMatrix[1, 3] = tempTransformation.amountY;
                            tempMatrix[2, 3] = tempTransformation.amountZ;
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "Scale":
                        {
                            tempMatrix[0, 0] = tempTransformation.amountX;
                            tempMatrix[1, 1] = tempTransformation.amountY;
                            tempMatrix[2, 2] = tempTransformation.amountZ;
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "ShearXY":
                        {
                            tempMatrix[0, 2] = tempTransformation.amountX;
                            tempMatrix[1, 2] = tempTransformation.amountY;
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "ShearYZ":
                        {
                            tempMatrix[0, 1] = tempTransformation.amountY;
                            tempMatrix[2, 1] = tempTransformation.amountZ;
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "ShearXZ":
                        {
                            tempMatrix[1, 0] = tempTransformation.amountX;
                            tempMatrix[2, 0] = tempTransformation.amountZ;
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "RotateX":
                        {
                            tempMatrix[1, 1] = Math.Cos(tempTransformation.theta);
                            tempMatrix[1, 2] = -Math.Sin(tempTransformation.theta);
                            tempMatrix[2, 1] = Math.Sin(tempTransformation.theta);
                            tempMatrix[2, 2] = Math.Cos(tempTransformation.theta);
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "RotateY":
                        {
                            tempMatrix[0, 0] = Math.Cos(tempTransformation.theta);
                            tempMatrix[0, 2] = Math.Sin(tempTransformation.theta);
                            tempMatrix[2, 0] = -Math.Sin(tempTransformation.theta);
                            tempMatrix[2, 2] = Math.Cos(tempTransformation.theta);
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                    case "RotateZ":
                        {
                            tempMatrix[0, 0] = Math.Cos(tempTransformation.theta);
                            tempMatrix[0, 1] = -Math.Sin(tempTransformation.theta);
                            tempMatrix[1, 0] = Math.Sin(tempTransformation.theta);
                            tempMatrix[1, 1] = Math.Cos(tempTransformation.theta);
                            transformationMatrices.Add(tempMatrix);
                            break;
                        }
                }
            }
            double[,] transformationMatrix = IdentityMatrix();
            foreach(double[,] matrices in transformationMatrices)
            {
                transformationMatrix = MatrixMultiplication(matrices, transformationMatrix);
            }
            return transformationMatrix;
        }

        public static List<Point3D> Transform(List<Point3D> cartesianPoints, List<Transformation> transformationList)
        {
            List<Point3D> result = new List<Point3D>();
            List<Transformation> transformations = new List<Transformation>();
            result.AddRange(cartesianPoints.Select(cartesian => new Point3D { X = cartesian.X, Y = cartesian.Y }));
            double[] coordinate = new double[4];
            Point3D transformationResult;
            double[,] transformationMatrix = TransformationsToTransformationMatrix(transformationList);
            for (int i = 0; i < result.Count; i++)
            {
                coordinate[0] = result[i].X;
                coordinate[1] = result[i].Y;
                coordinate[2] = result[i].Z;
                coordinate[3] = 1;
                coordinate = MatrixMultiplication(transformationMatrix, coordinate);
                transformationResult = new Point3D(Convert.ToSingle(Math.Round(coordinate[0], 3)), Convert.ToSingle(Math.Round(coordinate[1], 3)), Convert.ToSingle(Math.Round(coordinate[2],3)));
                result[i] = transformationResult;
            }
            return result;
        }

        public static double[,] IdentityMatrix ()
        {
            return new double[4, 4] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } };
        }
        public static double[,] MatrixMultiplication(double[,] matrix1, double[,] matrix2)
        {
            double[,] result = new double[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            return result;
        }
        public static double[] MatrixMultiplication(double[,] matrix1, double[] matrix2)
        {
            double[] result = new double[3];
            for (int i = 0; i < 4; i++)
            {
                result[i] = 0;
                for (int j = 0; j < 4; j++)
                {
                    result[i] += matrix1[i, j] * matrix2[j];
                }
            }
            return result;
        }
    }
}
