import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

const renderer = new THREE.WebGLRenderer({ canvas: document.getElementById('gameCanvas') });
renderer.outputColorSpace = THREE.SRGBColorSpace;

renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setClearColor(0x000000);
renderer.setPixelRatio(window.devicePixelRatio);

renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;

document.body.appendChild(renderer.domElement);

const scene = new THREE.Scene();

const camera = new THREE.PerspectiveCamera(30, window.innerWidth / window.innerHeight, 1, 1000);
camera.position.set(1, 1, 3);

const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.enablePan = false;
controls.minDistance = 1.2;
controls.maxDistance = 4;
controls.minPolarAngle = 0.0;
controls.maxPolarAngle = 1.5;
controls.autoRotate = true;
controls.target = new THREE.Vector3(0, 0.2, 0);
controls.update();

const groundGeometry = new THREE.PlaneGeometry(20, 20, 32, 32);
groundGeometry.rotateX(-Math.PI / 2);
const groundMaterial = new THREE.MeshStandardMaterial({
    color: 0x555555,
    side: THREE.DoubleSide
});
const groundMesh = new THREE.Mesh(groundGeometry, groundMaterial);
groundMesh.castShadow = false;
groundMesh.receiveShadow = true;
scene.add(groundMesh);

// Spotlight 1
const spotLight1 = new THREE.SpotLight(0x0000fa, 8, 100, 0.22, 1);
spotLight1.position.set(0, 4.5, 8); // Adjust position to shine from the side
spotLight1.castShadow = true;
spotLight1.shadow.bias = -0.0001;
spotLight1.target.position.set(0, 0.2, 0); // Ensure it points at the model
scene.add(spotLight1);
scene.add(spotLight1.target); // Add the target to the scene

// Spotlight 2
const spotLight2 = new THREE.SpotLight(0xffffff, 10, 200, 0.18, 1);
spotLight2.position.set(-2, 5, 2); // Adjust position to shine from another angle
spotLight2.castShadow = true;
spotLight2.shadow.bias = -0.0001;
spotLight2.target.position.set(0, 0.2, 0); // Ensure it points at the model
scene.add(spotLight2);
scene.add(spotLight2.target);

// Spotlight 3
const spotLight3 = new THREE.SpotLight(0xffffff, 10, 200, 0.18, 1);
spotLight3.position.set(0, 4.5, 8); // Adjust position to shine from a different angle
spotLight3.castShadow = true;
spotLight3.shadow.bias = -0.0001;
spotLight3.target.position.set(0, 0.2, 0); // Ensure it points at the model
scene.add(spotLight3);
scene.add(spotLight3.target); // Add the target to the scene

const spotLight3 = new THREE.SpotLight(0x00ffff, 1, 100, 0.22, 1);
spotLight3.position.set(0, 4.5, 0);
spotLight3.castShadow = true;
spotLight3.shadow.bias = -0.0001;
scene.add(spotLight3);

const loader = new GLTFLoader().setPath('/Assets/roboking/');
loader.load('roboking.glb', (gltf) => {
    const mesh = gltf.scene;

    mesh.traverse((child) => {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;
        }
    });

    mesh.position.set(0, 0.2, -0.05);
    mesh.rotateX(89.5);
    scene.add(mesh);

    document.getElementById('progress-container').style.display = 'none';
}, (xhr) => {
    document.getElementById('progress').innerHTML = `LOADING ${Math.max(xhr.loaded / xhr.total, 1) * 100}/100`;
},);

window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});

function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}

animate();